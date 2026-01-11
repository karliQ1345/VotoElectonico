using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Controllers;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Partidos;
using VotoElectonico.DTOs.Procesos;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcesosElectoralesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProcesosElectoralesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ---------- Helpers ----------
        private async Task<Usuario?> GetUsuarioFromVerifiedSession(Guid sessionId, CancellationToken ct)
        {
            var session = await _db.TwoFactorSessions
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

            if (session == null) return null;
            if (!session.Usado) return null;
            if (DateTime.UtcNow > session.ExpiraUtc) return null;

            return session.Usuario;
        }

        private async Task<ActionResult?> RequireAdmin(Guid sessionId, CancellationToken ct)
        {
            var user = await GetUsuarioFromVerifiedSession(sessionId, ct);
            if (user == null) return Unauthorized("Sesión inválida o no verificada.");
            if (user.Rol != RolUsuario.Administrador) return Forbid("Requiere rol Administrador.");
            return null;
        }

        // POST: api/ProcesosElectorales?sessionId=GUID
        [HttpPost]
        public async Task<ActionResult<ProcesoElectoralListDto>> Crear(
            [FromQuery] Guid sessionId,
            [FromBody] ProcesoElectoralCreateDto dto,
            CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest("El título es obligatorio.");

            if (dto.FechaFin <= dto.FechaInicio)
                return BadRequest("La fecha/hora fin debe ser mayor a la fecha/hora inicio.");

            //  REGLAS POR TIPO
            if (dto.Tipo == TipoEleccion.Presidencial)
            {
                //  Modalidad requerida
                if (!dto.ModalidadPresidencial.HasValue)
                    return BadRequest("ModalidadPresidencial es obligatoria cuando Tipo=Presidencial.");

                // En presidencial, no aplica plancha/nominal
                dto.PermitePlancha = false;
                dto.PermiteNominal = false;
                dto.MaxSeleccionNominal = null;
            }
            else if (dto.Tipo == TipoEleccion.Asambleistas)
            {
                // En asambleístas, ModalidadPresidencial debe ser null
                dto.ModalidadPresidencial = null;

                if (!dto.PermitePlancha && !dto.PermiteNominal)
                    return BadRequest("En Asambleístas debes permitir Plancha, Nominal o ambos.");

                if (dto.PermiteNominal)
                {
                    if (!dto.MaxSeleccionNominal.HasValue || dto.MaxSeleccionNominal.Value <= 0)
                        return BadRequest("MaxSeleccionNominal es obligatorio y debe ser > 0 cuando PermiteNominal=true.");
                }
                else
                {
                    dto.MaxSeleccionNominal = null;
                }
            }
            else
            {
                return BadRequest("Tipo de elección inválido.");
            }

            var proceso = new ProcesoElectoral
            {
                Titulo = dto.Titulo.Trim(),
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Tipo = dto.Tipo,
                Estado = EstadoProcesoElectoral.Pendiente,

                ModalidadPresidencial = dto.ModalidadPresidencial,

                PermitePlancha = dto.PermitePlancha,
                PermiteNominal = dto.PermiteNominal,
                MaxSeleccionNominal = dto.MaxSeleccionNominal
            };

            _db.ProcesosElectorales.Add(proceso);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Detalle), new { id = proceso.Id }, ToListDto(proceso));
        }

        // GET: api/ProcesosElectorales
        [HttpGet]
        public async Task<ActionResult<List<ProcesoElectoralListDto>>> Listar(
            [FromQuery] EstadoProcesoElectoral? estado = null,
            [FromQuery] TipoEleccion? tipo = null,
            CancellationToken ct = default)
        {
            var q = _db.ProcesosElectorales.AsNoTracking().AsQueryable();

            if (estado.HasValue) q = q.Where(p => p.Estado == estado.Value);
            if (tipo.HasValue) q = q.Where(p => p.Tipo == tipo.Value);

            var data = await q
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new ProcesoElectoralListDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    Estado = p.Estado,
                    Tipo = p.Tipo,

                    
                    ModalidadPresidencial = p.ModalidadPresidencial,

                    PermitePlancha = p.PermitePlancha,
                    PermiteNominal = p.PermiteNominal,
                    MaxSeleccionNominal = p.MaxSeleccionNominal
                })
                .ToListAsync(ct);

            return Ok(data);
        }

        // GET: api/ProcesosElectorales/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProcesoElectoralDetailDto>> Detalle(int id, CancellationToken ct)
        {
            var proceso = await _db.ProcesosElectorales
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProcesoElectoralDetailDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    Estado = p.Estado,
                    Tipo = p.Tipo,

                    
                    ModalidadPresidencial = p.ModalidadPresidencial,

                    PermitePlancha = p.PermitePlancha,
                    PermiteNominal = p.PermiteNominal,
                    MaxSeleccionNominal = p.MaxSeleccionNominal,
                    PartidosRegistrados = p.PartidosInscritos.Count,
                    VotosRegistrados = p.UrnaDeVotos.Count,
                    TotalVotantesQueYaVotaron = p.HistorialLog.Count
                })
                .FirstOrDefaultAsync(ct);

            if (proceso == null) return NotFound("Proceso electoral no encontrado.");
            return Ok(proceso);
        }

        [HttpGet("activo")]
        public async Task<ActionResult<ProcesoElectoralListDto>> ProcesoActivo(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var proceso = await _db.ProcesosElectorales
                .AsNoTracking()
                .Where(p => p.Estado == EstadoProcesoElectoral.Activo &&
                            p.FechaInicio <= now && p.FechaFin >= now)
                .OrderBy(p => p.FechaFin)
                .Select(p => new ProcesoElectoralListDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    Estado = p.Estado,
                    Tipo = p.Tipo,

                   
                    ModalidadPresidencial = p.ModalidadPresidencial,

                    PermitePlancha = p.PermitePlancha,
                    PermiteNominal = p.PermiteNominal,
                    MaxSeleccionNominal = p.MaxSeleccionNominal
                })
                .FirstOrDefaultAsync(ct);

            if (proceso == null) return NotFound("No hay proceso activo actualmente.");
            return Ok(proceso);
        }

        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            [FromQuery] Guid sessionId,
            [FromBody] CambiarEstadoProcesoDto dto,
            CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (proceso == null) return NotFound("Proceso electoral no encontrado.");

            var now = DateTime.UtcNow;

            switch (dto.Accion)
            {
                case AccionEstadoProceso.Activar:
                    if (proceso.Estado == EstadoProcesoElectoral.Finalizado)
                        return BadRequest("No se puede activar un proceso finalizado.");
                    if (now > proceso.FechaFin)
                        return BadRequest("No se puede activar: la fecha/hora fin ya pasó.");
                    proceso.Estado = EstadoProcesoElectoral.Activo;
                    break;

                case AccionEstadoProceso.Finalizar:
                    if (proceso.Estado == EstadoProcesoElectoral.Finalizado)
                        return BadRequest("El proceso ya está finalizado.");
                    proceso.Estado = EstadoProcesoElectoral.Finalizado;
                    break;

                case AccionEstadoProceso.Suspender:
                    if (proceso.Estado != EstadoProcesoElectoral.Activo)
                        return BadRequest("Solo puedes suspender un proceso Activo.");
                    proceso.Estado = EstadoProcesoElectoral.Suspendido;
                    break;

                case AccionEstadoProceso.Reanudar:
                    if (proceso.Estado != EstadoProcesoElectoral.Suspendido)
                        return BadRequest("Solo puedes reanudar un proceso Suspendido.");
                    if (now > proceso.FechaFin)
                        return BadRequest("No se puede reanudar: la fecha/hora fin ya pasó.");
                    proceso.Estado = EstadoProcesoElectoral.Activo;
                    break;

                default:
                    return BadRequest("Acción inválida.");
            }

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        private static ProcesoElectoralListDto ToListDto(ProcesoElectoral p) => new()
        {
            Id = p.Id,
            Titulo = p.Titulo,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Estado = p.Estado,
            Tipo = p.Tipo,

            ModalidadPresidencial = p.ModalidadPresidencial,

            PermitePlancha = p.PermitePlancha,
            PermiteNominal = p.PermiteNominal,
            MaxSeleccionNominal = p.MaxSeleccionNominal
        };
    }
}
