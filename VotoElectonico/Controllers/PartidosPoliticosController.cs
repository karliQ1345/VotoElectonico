using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Partidos;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidosPoliticosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PartidosPoliticosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helpers (sesión 2FA verificada)

        private async Task<Usuario?> GetUsuarioFromVerifiedSession(Guid sessionId, CancellationToken ct)
        {
            var session = await _db.TwoFactorSessions
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

            if (session == null) return null;
            if (!session.Usado) return null;                 // no verificada
            if (DateTime.UtcNow > session.ExpiraUtc) return null; // expirada

            return session.Usuario;
        }

        private async Task<ActionResult?> RequireAdmin(Guid sessionId, CancellationToken ct)
        {
            var user = await GetUsuarioFromVerifiedSession(sessionId, ct);
            if (user == null) return Unauthorized("Sesión inválida o no verificada.");
            if (user.Rol != RolUsuario.Administrador) return Forbid("Requiere rol Administrador.");
            return null; // OK
        }

        // Público: listar (opcional por procesoId)
        // GET: api/PartidosPoliticos?procesoId=1

        [HttpGet]
        public async Task<ActionResult<List<PartidoPoliticoListDto>>> List([FromQuery] int? procesoId, CancellationToken ct)
        {
            var q = _db.PartidosPoliticos
                .AsNoTracking()
                .AsQueryable();

            if (procesoId.HasValue)
                q = q.Where(p => p.ProcesoElectoralId == procesoId.Value);

            var items = await q
                .OrderBy(p => p.NumeroLista)
                .Select(p => new PartidoPoliticoListDto
                {
                    Id = p.Id,
                    ProcesoElectoralId = p.ProcesoElectoralId,
                    NombreLista = p.NombreLista,
                    NumeroLista = p.NumeroLista,
                    LogoUrl = p.LogoUrl,
                    TotalCandidatos = p.Candidatos.Count
                })
                .ToListAsync(ct);

            return Ok(items);
        }


        // Admin: crear
        // POST: api/PartidosPoliticos?sessionId=GUID

        [HttpPost]
        public async Task<ActionResult<PartidoPoliticoListDto>> Create(
            [FromQuery] Guid sessionId,
            [FromBody] PartidoPoliticoCreateDto dto,
            CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(p => p.Id == dto.ProcesoElectoralId, ct);
            if (proceso == null) return BadRequest("ProcesoElectoralId no existe.");

            // Recomendado: evitar repetir NumeroLista en el mismo proceso
            var existeNumero = await _db.PartidosPoliticos
                .AnyAsync(p => p.ProcesoElectoralId == dto.ProcesoElectoralId && p.NumeroLista == dto.NumeroLista, ct);

            if (existeNumero) return BadRequest("Ya existe un partido con ese NúmeroLista en este proceso.");

            var partido = new PartidoPolitico
            {
                ProcesoElectoralId = dto.ProcesoElectoralId,
                NombreLista = dto.NombreLista.Trim(),
                NumeroLista = dto.NumeroLista,
                LogoUrl = dto.LogoUrl
            };

            _db.PartidosPoliticos.Add(partido);
            await _db.SaveChangesAsync(ct);

            return Ok(new PartidoPoliticoListDto
            {
                Id = partido.Id,
                ProcesoElectoralId = partido.ProcesoElectoralId,
                NombreLista = partido.NombreLista,
                NumeroLista = partido.NumeroLista,
                LogoUrl = partido.LogoUrl,
                TotalCandidatos = 0
            });
        }


        // Admin: actualizar
        // PUT: api/PartidosPoliticos/{id}?sessionId=GUID

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromQuery] Guid sessionId,
            [FromBody] PartidoPoliticoUpdateDto dto,
            CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var partido = await _db.PartidosPoliticos.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (partido == null) return NotFound("Partido no encontrado.");

            // Evitar duplicar NumeroLista en el mismo proceso (excepto el mismo partido)
            var existeNumero = await _db.PartidosPoliticos.AnyAsync(p =>
                p.ProcesoElectoralId == partido.ProcesoElectoralId &&
                p.NumeroLista == dto.NumeroLista &&
                p.Id != partido.Id, ct);

            if (existeNumero) return BadRequest("Ya existe otro partido con ese NúmeroLista en este proceso.");

            partido.NombreLista = dto.NombreLista.Trim();
            partido.NumeroLista = dto.NumeroLista;
            partido.LogoUrl = dto.LogoUrl;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

    
        // Admin: eliminar
        // DELETE: api/PartidosPoliticos/{id}?sessionId=GUID
 
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] Guid sessionId, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var partido = await _db.PartidosPoliticos.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (partido == null) return NotFound("Partido no encontrado.");

            _db.PartidosPoliticos.Remove(partido);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
