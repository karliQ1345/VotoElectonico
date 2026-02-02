using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Procesos;
using VotoElectonico.Models;
using VotoElectonico.Models.Enums;

namespace VotoElectonico.Controllers
{
    [Authorize(Roles = nameof(RolTipo.Administrador))]
    [ApiController]
    [Route("api/procesos")]
    public class ProcesosController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        public ProcesosController(ApplicationDbContext db) => _db = db;

        [Authorize(Roles = nameof(RolTipo.Administrador))]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<IdResponseDto>>> Crear([FromBody] CrearProcesoRequestDto req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Nombre))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("Nombre requerido."));

            var inicioUtc = req.InicioUtc.Kind switch
            {
                DateTimeKind.Utc => req.InicioUtc,
                DateTimeKind.Local => req.InicioUtc.ToUniversalTime(),
                _ => DateTime.SpecifyKind(req.InicioUtc, DateTimeKind.Local).ToUniversalTime() // Unspecified => asumimos local
            };

            var finUtc = req.FinUtc.Kind switch
            {
                DateTimeKind.Utc => req.FinUtc,
                DateTimeKind.Local => req.FinUtc.ToUniversalTime(),
                _ => DateTime.SpecifyKind(req.FinUtc, DateTimeKind.Local).ToUniversalTime()
            };

            if (finUtc <= inicioUtc)
                return BadRequest(ApiResponse<IdResponseDto>.Fail("FinUtc debe ser mayor a InicioUtc."));

            var p = new ProcesoElectoral
            {
                Id = Guid.NewGuid(),
                Nombre = req.Nombre.Trim(),
                InicioUtc = inicioUtc,
                FinUtc = finUtc,
                Estado = ProcesoEstado.Pendiente,
                CreadoUtc = DateTime.UtcNow
            };

            _db.ProcesosElectorales.Add(p);
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = p.Id.ToString() }, "Proceso creado."));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProcesoResumenDto>>>> Listar(CancellationToken ct)
        {
            var list = await _db.ProcesosElectorales
                .OrderByDescending(x => x.CreadoUtc)
                .Select(x => new ProcesoResumenDto
                {
                    ProcesoElectoralId = x.Id.ToString(),
                    Nombre = x.Nombre,
                    Estado = x.Estado.ToString(),
                    InicioUtc = x.InicioUtc,
                    FinUtc = x.FinUtc,
                    PadronCargado = _db.PadronRegistros.Any(pr => pr.ProcesoElectoralId == x.Id)
                })
                .ToListAsync(ct);

            return Ok(ApiResponse<List<ProcesoResumenDto>>.Success(list));
        }

        [Authorize(Roles = nameof(RolTipo.Administrador))]
        [HttpPost("{procesoId:guid}/activar")]
        public async Task<ActionResult<ApiResponse<string>>> Activar(Guid procesoId, CancellationToken ct)
        {
            var p = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (p == null) return NotFound(ApiResponse<string>.Fail("Proceso no existe."));

            if (p.Estado == ProcesoEstado.Finalizado)
                return BadRequest(ApiResponse<string>.Fail("No se puede activar un proceso finalizado."));

            var tienePadron = await _db.PadronRegistros.AnyAsync(x => x.ProcesoElectoralId == procesoId, ct);
            if (!tienePadron)
                return BadRequest(ApiResponse<string>.Fail("No se puede activar: primero cargue el padrón electoral."));

            p.Estado = ProcesoEstado.Activo;
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<string>.Success("OK", "Proceso activado."));
        }

        [Authorize(Roles = nameof(RolTipo.Administrador))]
        [HttpPost("{procesoId:guid}/finalizar")]
        public async Task<ActionResult<ApiResponse<string>>> Finalizar(Guid procesoId, CancellationToken ct)
        {
            var p = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (p == null) return NotFound(ApiResponse<string>.Fail("Proceso no existe."));

            p.Estado = ProcesoEstado.Finalizado;
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<string>.Success("OK", "Proceso finalizado."));
        }

        [Authorize]
        [HttpGet("activo")]
        public async Task<ActionResult<ApiResponse<ProcesoActivoDto>>> GetActivo(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var p = await _db.ProcesosElectorales
                .Where(x => x.Estado == ProcesoEstado.Activo
                         && now >= x.InicioUtc
                         && now <= x.FinUtc)
                .OrderByDescending(x => x.CreadoUtc)
                .Select(x => new ProcesoActivoDto
                {
                    ProcesoElectoralId = x.Id.ToString(),
                    Nombre = x.Nombre,
                    InicioUtc = x.InicioUtc,
                    FinUtc = x.FinUtc,
                    Estado = x.Estado.ToString()
                })
                .FirstOrDefaultAsync(ct);

            if (p == null)
                return Ok(ApiResponse<ProcesoActivoDto>.Fail("No hay proceso activo en este momento."));

            return Ok(ApiResponse<ProcesoActivoDto>.Success(p));
        }
    }
}
