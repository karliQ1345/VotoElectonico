using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Procesos;
using VotoElectonico.Models.Enums;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/public/procesos")]
    public class PublicProcesosController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        public PublicProcesosController(ApplicationDbContext db) => _db = db;

        // GET api/public/procesos/activo
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
