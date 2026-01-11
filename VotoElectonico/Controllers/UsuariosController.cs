using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Usuarios;
using VotoElectonico.Models;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UsuariosController(ApplicationDbContext db) => _db = db;

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

        // GET: api/Usuarios?sessionId=GUID&rol=Votante
        [HttpGet]
        public async Task<ActionResult<List<UsuarioListDto>>> Listar([FromQuery] Guid sessionId, [FromQuery] RolUsuario? rol, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var q = _db.Usuarios.AsNoTracking().AsQueryable();
            if (rol.HasValue) q = q.Where(u => u.Rol == rol.Value);

            var data = await q
                .OrderBy(u => u.NombresCompletos)
                .Select(u => new UsuarioListDto
                {
                    Id = u.Id,
                    Cedula = u.Cedula,
                    Rol = u.Rol,
                    CorreoElectronico = u.CorreoElectronico,
                    NombresCompletos = u.NombresCompletos,
                    Genero = u.Genero,
                    Provincia = u.Provincia,
                    Canton = u.Canton,
                    Parroquia = u.Parroquia,
                    FotoUrl = u.FotoUrl
                })
                .ToListAsync(ct);

            return Ok(data);
        }

        // GET: api/Usuarios/por-cedula/0102030405?sessionId=GUID
        [HttpGet("por-cedula/{cedula}")]
        public async Task<ActionResult<UsuarioListDto>> ObtenerPorCedula(string cedula, [FromQuery] Guid sessionId, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var u = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Cedula == cedula, ct);
            if (u == null) return NotFound("Usuario no encontrado.");

            return Ok(new UsuarioListDto
            {
                Id = u.Id,
                Cedula = u.Cedula,
                Rol = u.Rol,
                CorreoElectronico = u.CorreoElectronico,
                NombresCompletos = u.NombresCompletos,
                Genero = u.Genero,
                Provincia = u.Provincia,
                Canton = u.Canton,
                Parroquia = u.Parroquia,
                FotoUrl = u.FotoUrl
            });
        }

        // POST: api/Usuarios?sessionId=GUID
        [HttpPost]
        public async Task<ActionResult<UsuarioListDto>> Crear([FromQuery] Guid sessionId, [FromBody] UsuarioCreateDto dto, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var existe = await _db.Usuarios.AnyAsync(u => u.Cedula == dto.Cedula, ct);
            if (existe) return BadRequest("Ya existe un usuario con esa cédula.");

            var user = new Usuario
            {
                Cedula = dto.Cedula,
                Rol = dto.Rol,
                CorreoElectronico = dto.CorreoElectronico.Trim(),
                NombresCompletos = dto.NombresCompletos.Trim(),
                Genero = dto.Genero,
                Provincia = dto.Provincia.Trim(),
                Canton = dto.Canton.Trim(),
                Parroquia = dto.Parroquia.Trim(),
                FotoUrl = dto.FotoUrl
            };

            _db.Usuarios.Add(user);
            await _db.SaveChangesAsync(ct);

            return Ok(new UsuarioListDto
            {
                Id = user.Id,
                Cedula = user.Cedula,
                Rol = user.Rol,
                CorreoElectronico = user.CorreoElectronico,
                NombresCompletos = user.NombresCompletos,
                Genero = user.Genero,
                Provincia = user.Provincia,
                Canton = user.Canton,
                Parroquia = user.Parroquia,
                FotoUrl = user.FotoUrl
            });
        }

        // POST: api/Usuarios/carga-masiva?sessionId=GUID
        // (Para tus pruebas en Swagger: envías una lista JSON)
        [HttpPost("carga-masiva")]
        public async Task<IActionResult> CargaMasiva([FromQuery] Guid sessionId, [FromBody] List<UsuarioCreateDto> usuarios, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            foreach (var dto in usuarios)
            {
                if (await _db.Usuarios.AnyAsync(u => u.Cedula == dto.Cedula, ct))
                    continue; // si ya existe, lo saltamos

                _db.Usuarios.Add(new Usuario
                {
                    Cedula = dto.Cedula,
                    Rol = dto.Rol,
                    CorreoElectronico = dto.CorreoElectronico.Trim(),
                    NombresCompletos = dto.NombresCompletos.Trim(),
                    Genero = dto.Genero,
                    Provincia = dto.Provincia.Trim(),
                    Canton = dto.Canton.Trim(),
                    Parroquia = dto.Parroquia.Trim(),
                    FotoUrl = dto.FotoUrl
                });
            }

            await _db.SaveChangesAsync(ct);
            return Ok("Carga masiva completada (se omitieron cédulas duplicadas).");
        }

        // PUT: api/Usuarios/{id}?sessionId=GUID
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromQuery] Guid sessionId, [FromBody] UsuarioUpdateDto dto, CancellationToken ct)
        {
            var guard = await RequireAdmin(sessionId, ct);
            if (guard != null) return guard;

            var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u == null) return NotFound("Usuario no encontrado.");

            u.CorreoElectronico = dto.CorreoElectronico.Trim();
            u.NombresCompletos = dto.NombresCompletos.Trim();
            u.Genero = dto.Genero;
            u.Provincia = dto.Provincia.Trim();
            u.Canton = dto.Canton.Trim();
            u.Parroquia = dto.Parroquia.Trim();
            u.FotoUrl = dto.FotoUrl;
            u.Rol = dto.Rol;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }


}
