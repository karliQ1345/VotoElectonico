using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Auth;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;

namespace VotoElectonico.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/me")]
    public class MeController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        public MeController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<MeResponseDto>>> Get(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(ApiResponse<MeResponseDto>.Fail("Token inválido."));

            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == userId.Value, ct);
            if (user == null) return Unauthorized(ApiResponse<MeResponseDto>.Fail("Usuario no existe."));

            var roles = await _db.UsuarioRoles.Where(r => r.UsuarioId == user.Id).Select(r => r.Rol.ToString()).ToListAsync(ct);

            var resp = new MeResponseDto
            {
                UsuarioId = user.Id.ToString(),
                Cedula = user.Cedula,
                NombreCompleto = user.NombreCompleto,
                Roles = roles
            };

            return Ok(ApiResponse<MeResponseDto>.Success(resp));
        }
    }
}
