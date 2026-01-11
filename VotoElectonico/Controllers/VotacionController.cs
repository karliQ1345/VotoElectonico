using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Votación;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotacionController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public VotacionController(ApplicationDbContext db) => _db = db;

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

        [HttpGet("estado")]
        public async Task<ActionResult<EstadoVotoDto>> Estado([FromQuery] Guid sessionId, [FromQuery] int procesoId, CancellationToken ct)
        {
            var user = await GetUsuarioFromVerifiedSession(sessionId, ct);
            if (user == null) return Unauthorized("Sesión inválida o no verificada.");

            var hist = await _db.HistorialVotaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.UsuarioId == user.Id && h.ProcesoElectoralId == procesoId, ct);

            if (hist == null)
                return Ok(new EstadoVotoDto { YaVoto = false });

            return Ok(new EstadoVotoDto
            {
                YaVoto = true,
                FechaSufragioUtc = hist.FechaSufragioUtc,
                CodigoCertificado = hist.CodigoCertificado
            });
        }

        [HttpPost("emitir")]
        public async Task<ActionResult<EmitirVotoResponseDto>> Emitir([FromQuery] Guid sessionId, [FromBody] EmitirVotoRequestDto dto, CancellationToken ct)
        {
            var user = await GetUsuarioFromVerifiedSession(sessionId, ct);
            if (user == null) return Unauthorized("Sesión inválida o no verificada.");

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(p => p.Id == dto.ProcesoElectoralId, ct);
            if (proceso == null) return NotFound("Proceso electoral no existe.");

            var now = DateTime.UtcNow;

            if (proceso.Estado != EstadoProcesoElectoral.Activo)
                return BadRequest("El proceso electoral no está activo.");

            if (!(proceso.FechaInicio <= now && now <= proceso.FechaFin))
                return BadRequest("Fuera del rango de votación del proceso.");

            var yaVoto = await _db.HistorialVotaciones.AnyAsync(h => h.UsuarioId == user.Id && h.ProcesoElectoralId == proceso.Id, ct);
            if (yaVoto) return BadRequest("El usuario ya votó en este proceso.");

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest("Debe enviar al menos un detalle de voto.");

            foreach (var d in dto.Detalles)
            {
                if (d.PartidoPoliticoId.HasValue && d.CandidatoId.HasValue)
                    return BadRequest("Un detalle no puede tener PartidoPoliticoId y CandidatoId al mismo tiempo.");
            }

            // REGLAS POR TIPO
            if (proceso.Tipo == TipoEleccion.Presidencial)
            {
                if (!proceso.ModalidadPresidencial.HasValue)
                    return BadRequest("El proceso presidencial no tiene ModalidadPresidencial configurada.");

                if (dto.Detalles.Count != 1)
                    return BadRequest("En Presidencial debe enviarse exactamente 1 selección.");

                var d = dto.Detalles[0];

                switch (proceso.ModalidadPresidencial.Value)
                {
                    case ModalidadPresidencial.PorCandidato:
                        if (d.Tipo != TipoDetalleVoto.Candidato || !d.CandidatoId.HasValue)
                            return BadRequest("Debe votar por un candidato (Tipo=Candidato y CandidatoId).");
                        if (d.PartidoPoliticoId.HasValue)
                            return BadRequest("En voto por candidato no envíe PartidoPoliticoId.");

                        // candidato debe pertenecer al proceso
                        var okCand = await _db.Candidatos.AnyAsync(c => c.Id == d.CandidatoId.Value && c.Partido.ProcesoElectoralId == proceso.Id, ct);
                        if (!okCand) return BadRequest("El candidato no pertenece a este proceso.");
                        break;

                    case ModalidadPresidencial.PorCandidatoConBlanco:
                        if (d.Tipo == TipoDetalleVoto.Blanco)
                        {
                            if (d.CandidatoId.HasValue || d.PartidoPoliticoId.HasValue)
                                return BadRequest("BLANCO no debe incluir IDs.");
                        }
                        else
                        {
                            if (d.Tipo != TipoDetalleVoto.Candidato || !d.CandidatoId.HasValue)
                                return BadRequest("Debe votar por un candidato o BLANCO.");
                            var okCand2 = await _db.Candidatos.AnyAsync(c => c.Id == d.CandidatoId.Value && c.Partido.ProcesoElectoralId == proceso.Id, ct);
                            if (!okCand2) return BadRequest("El candidato no pertenece a este proceso.");
                        }
                        break;

                    case ModalidadPresidencial.SiNo:
                        if (d.Tipo != TipoDetalleVoto.Si && d.Tipo != TipoDetalleVoto.No)
                            return BadRequest("Debe seleccionar SI o NO.");
                        if (d.CandidatoId.HasValue || d.PartidoPoliticoId.HasValue)
                            return BadRequest("SI/NO no debe incluir IDs.");
                        break;

                    case ModalidadPresidencial.SiNoConBlanco:
                        if (d.Tipo != TipoDetalleVoto.Si && d.Tipo != TipoDetalleVoto.No && d.Tipo != TipoDetalleVoto.Blanco)
                            return BadRequest("Debe seleccionar SI, NO o BLANCO.");
                        if (d.CandidatoId.HasValue || d.PartidoPoliticoId.HasValue)
                            return BadRequest("SI/NO/BLANCO no debe incluir IDs.");
                        break;

                    default:
                        return BadRequest("ModalidadPresidencial inválida.");
                }
            }
            else // Asambleistas
            {
                var tienePlancha = dto.Detalles.Any(x => x.Tipo == TipoDetalleVoto.Partido);
                var tieneNominal = dto.Detalles.Any(x => x.Tipo == TipoDetalleVoto.Candidato);

                if (tienePlancha && tieneNominal)
                    return BadRequest("No puede mezclar Plancha y Nominal en el mismo voto.");

                if (tienePlancha)
                {
                    if (!proceso.PermitePlancha) return BadRequest("Este proceso no permite voto por plancha.");
                    if (dto.Detalles.Count != 1) return BadRequest("Plancha debe ser exactamente 1 selección.");
                    var d = dto.Detalles[0];
                    if (!d.PartidoPoliticoId.HasValue) return BadRequest("Debe enviar PartidoPoliticoId.");
                    var okPartido = await _db.PartidosPoliticos.AnyAsync(p => p.Id == d.PartidoPoliticoId.Value && p.ProcesoElectoralId == proceso.Id, ct);
                    if (!okPartido) return BadRequest("El partido no pertenece a este proceso.");
                }
                else if (tieneNominal)
                {
                    if (!proceso.PermiteNominal) return BadRequest("Este proceso no permite voto nominal.");

                    var max = proceso.MaxSeleccionNominal ?? 0;
                    if (max <= 0) return BadRequest("El proceso no tiene MaxSeleccionNominal configurado.");

                    var candIds = dto.Detalles.Select(x => x.CandidatoId).ToList();
                    if (candIds.Any(x => !x.HasValue)) return BadRequest("Cada voto nominal requiere CandidatoId.");
                    if (dto.Detalles.Count > max) return BadRequest($"Solo puede seleccionar hasta {max} candidatos.");

                    var distinct = candIds.Select(x => x!.Value).Distinct().Count();
                    if (distinct != dto.Detalles.Count) return BadRequest("No puede repetir el mismo candidato.");

                    var ids = candIds.Select(x => x!.Value).ToList();
                    var countOk = await _db.Candidatos.CountAsync(c => ids.Contains(c.Id) && c.Partido.ProcesoElectoralId == proceso.Id, ct);
                    if (countOk != ids.Count) return BadRequest("Uno o más candidatos no pertenecen a este proceso.");
                }
                else
                {
                    return BadRequest("En Asambleístas debes votar por Plancha o por Nominal.");
                }
            }

            // Crear voto anónimo
            var voto = new Voto
            {
                ProcesoElectoralId = proceso.Id,
                FechaIngresoUtc = DateTime.UtcNow,

                GeneroVotante = user.Genero,
                ProvinciaVotante = user.Provincia,
                CantonVotante = user.Canton,
                ParroquiaVotante = user.Parroquia,

                // Hash de integridad (sin userId para evitar cualquier vínculo innecesario)
                HashSeguridad = BuildHash(dto, proceso.Id)
            };

            _db.Votos.Add(voto);
            await _db.SaveChangesAsync(ct);

            foreach (var d in dto.Detalles)
            {
                _db.DetalleVotos.Add(new DetalleVoto
                {
                    VotoId = voto.Id,
                    Tipo = d.Tipo,
                    PartidoPoliticoId = d.PartidoPoliticoId,
                    CandidatoId = d.CandidatoId
                });
            }

            var cert = Guid.NewGuid().ToString();
            _db.HistorialVotaciones.Add(new HistorialVotacion
            {
                UsuarioId = user.Id,
                ProcesoElectoralId = proceso.Id,
                FechaSufragioUtc = DateTime.UtcNow,
                CodigoCertificado = cert
            });

            await _db.SaveChangesAsync(ct);

            return Ok(new EmitirVotoResponseDto
            {
                CodigoCertificado = cert,
                FechaSufragioUtc = DateTime.UtcNow
            });
        }

        private static string BuildHash(EmitirVotoRequestDto dto, int procesoId)
        {
            var payload = new
            {
                procesoId,
                dto.Detalles,
                ts = DateTime.UtcNow,
                salt = Guid.NewGuid().ToString("N")
            };

            var json = JsonSerializer.Serialize(payload);
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(bytes);
        }
    }
}
