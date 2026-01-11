using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Reportes;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ReportesController(ApplicationDbContext db) => _db = db;

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

        // GET: api/Reportes/general/1?sessionId=GUID
        // Votante: general | Admin/Candidato: también puede
        [HttpGet("general/{procesoId:int}")]
        public async Task<ActionResult<ReporteGeneralDto>> General(int procesoId, [FromQuery] Guid sessionId, CancellationToken ct)
        {
            var user = await GetUsuarioFromVerifiedSession(sessionId, ct);
            if (user == null) return Unauthorized("Sesión inválida o no verificada.");

            var existe = await _db.ProcesosElectorales.AsNoTracking().AnyAsync(p => p.Id == procesoId, ct);
            if (!existe) return NotFound("Proceso no existe.");

            var totalVotos = await _db.Votos.AsNoTracking().CountAsync(v => v.ProcesoElectoralId == procesoId, ct);
            var totalVotaron = await _db.HistorialVotaciones.AsNoTracking().CountAsync(h => h.ProcesoElectoralId == procesoId, ct);

            // Conteo por TipoDetalleVoto
            var porTipo = await _db.DetalleVotos
                .AsNoTracking()
                .Where(d => d.Voto.ProcesoElectoralId == procesoId)
                .GroupBy(d => d.Tipo)
                .Select(g => new ItemConteoDto { Clave = g.Key.ToString(), Conteo = g.Count() })
                .ToListAsync(ct);

            // Conteo por Partido (plancha)
            var porPartido = await _db.DetalleVotos
                .AsNoTracking()
                .Where(d => d.Voto.ProcesoElectoralId == procesoId && d.PartidoPoliticoId != null)
                .GroupBy(d => d.Partido!.NombreLista)
                .Select(g => new ItemConteoDto { Clave = g.Key, Conteo = g.Count() })
                .ToListAsync(ct);

            // General: por candidato lo dejamos vacío (o lo puedes activar)
            return Ok(new ReporteGeneralDto
            {
                ProcesoElectoralId = procesoId,
                TotalVotos = totalVotos,
                TotalVotantesQueYaVotaron = totalVotaron,
                PorTipo = porTipo,
                PorPartido = porPartido,
                PorCandidato = new List<ItemConteoDto>()
            });
        }

        // GET: api/Reportes/detallado/1?sessionId=GUID
        // Solo Admin o Candidato
        [HttpGet("detallado/{procesoId:int}")]
        public async Task<ActionResult<ReporteGeneralDto>> Detallado(int procesoId, [FromQuery] Guid sessionId, CancellationToken ct)
        {
            var user = await GetUsuarioFromVerifiedSession(sessionId, ct);
            if (user == null) return Unauthorized("Sesión inválida o no verificada.");

            if (user.Rol != RolUsuario.Administrador && user.Rol != RolUsuario.Candidato)
                return Forbid("Requiere rol Administrador o Candidato.");

            var existe = await _db.ProcesosElectorales.AsNoTracking().AnyAsync(p => p.Id == procesoId, ct);
            if (!existe) return NotFound("Proceso no existe.");

            var totalVotos = await _db.Votos.AsNoTracking().CountAsync(v => v.ProcesoElectoralId == procesoId, ct);
            var totalVotaron = await _db.HistorialVotaciones.AsNoTracking().CountAsync(h => h.ProcesoElectoralId == procesoId, ct);

            var porTipo = await _db.DetalleVotos
                .AsNoTracking()
                .Where(d => d.Voto.ProcesoElectoralId == procesoId)
                .GroupBy(d => d.Tipo)
                .Select(g => new ItemConteoDto { Clave = g.Key.ToString(), Conteo = g.Count() })
                .ToListAsync(ct);

            var porPartido = await _db.DetalleVotos
                .AsNoTracking()
                .Where(d => d.Voto.ProcesoElectoralId == procesoId && d.PartidoPoliticoId != null)
                .GroupBy(d => d.Partido!.NombreLista)
                .Select(g => new ItemConteoDto { Clave = g.Key, Conteo = g.Count() })
                .ToListAsync(ct);

            var porCandidato = await _db.DetalleVotos
                .AsNoTracking()
                .Where(d => d.Voto.ProcesoElectoralId == procesoId && d.CandidatoId != null)
                .GroupBy(d => d.Candidato!.NombreEnPapeleta)
                .Select(g => new ItemConteoDto { Clave = g.Key, Conteo = g.Count() })
                .ToListAsync(ct);

            return Ok(new ReporteGeneralDto
            {
                ProcesoElectoralId = procesoId,
                TotalVotos = totalVotos,
                TotalVotantesQueYaVotaron = totalVotaron,
                PorTipo = porTipo,
                PorPartido = porPartido,
                PorCandidato = porCandidato
            });
        }
    }

}
