using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Candidatos;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    public class CandidatosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CandidatosController(ApplicationDbContext db) => _db = db;

        // Helpers
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

        // GET: api/Candidatos?procesoId=1
        [HttpGet]
        public async Task<ActionResult<List<CandidatoListDto>>> ListarPorProceso([FromQuery] int procesoId, CancellationToken ct)
        {
            var data = await _db.Candidatos
                .AsNoTracking()
                .Where(c => c.Partido.ProcesoElectoralId == procesoId)
                .OrderBy(c => c.Partido.NumeroLista)
                .ThenBy(c => c.OrdenEnLista)
                .Select(c => new CandidatoListDto
                {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    NombreEnPapeleta = c.NombreEnPapeleta,
                    FotoUrl = c.FotoUrl,
                    OrdenEnLista = c.OrdenEnLista,
                    PartidoPoliticoId = c.PartidoPoliticoId,
                    PartidoNombre = c.Partido.NombreLista,
                    ProcesoElectoralId = c.Partido.ProcesoElectoralId
                })
                .ToListAsync(ct);

            return Ok(data);
        }

        // GET: api/Candidatos/por-partido?partidoId=10
        [HttpGet("por-partido")]
        public async Task<ActionResult<List<CandidatoListDto>>> ListarPorPartido([FromQuery] int partidoId, CancellationToken ct)
        {
            var data = await _db.Candidatos
                .AsNoTracking()
                .Where(c => c.PartidoPoliticoId == partidoId)
                .OrderBy(c => c.OrdenEnLista)
                .Select(c => new CandidatoListDto
                {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    NombreEnPapeleta = c.NombreEnPapeleta,
                    FotoUrl = c.FotoUrl,
                    OrdenEnLista = c.OrdenEnLista,
                    PartidoPoliticoId = c.PartidoPoliticoId,
                    PartidoNombre = c.Partido.NombreLista,
                    ProcesoElectoralId = c.Partido.ProcesoElectoralId
                })
                .ToListAsync(ct);

            return Ok(data);
        }

        // POST: api/Candidatos?sessionId=GUID (Admin)
        [HttpPost]
        public async Task<IActionResult> Crear([FromQuery] Guid sessionId, [FromBody] CandidatoCreateDto dto, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.UsuarioId, ct);
            if (usuario == null) return NotFound("Usuario no existe.");

            if (usuario.Rol != RolUsuario.Candidato)
                return BadRequest("El usuario debe tener rol Candidato para registrarlo como candidato.");

            var partido = await _db.PartidosPoliticos.FirstOrDefaultAsync(p => p.Id == dto.PartidoPoliticoId, ct);
            if (partido == null) return NotFound("Partido no existe.");

            // El índice único en Candidato.UsuarioId ya evita duplicados, pero damos mensaje claro:
            var yaEsCandidato = await _db.Candidatos.AnyAsync(c => c.UsuarioId == dto.UsuarioId, ct);
            if (yaEsCandidato) return BadRequest("Este usuario ya está registrado como candidato.");

            var candi = new Candidato
            {
                UsuarioId = dto.UsuarioId,
                PartidoPoliticoId = dto.PartidoPoliticoId,
                NombreEnPapeleta = dto.NombreEnPapeleta.Trim(),
                FotoUrl = dto.FotoUrl,
                OrdenEnLista = dto.OrdenEnLista
            };

            _db.Candidatos.Add(candi);
            await _db.SaveChangesAsync(ct);

            return Ok("Candidato registrado.");
        }
    }

}

