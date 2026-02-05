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

        [HttpGet("ultimo-finalizado")]
        public async Task<ActionResult<ApiResponse<ProcesoActivoDto>>> GetUltimoFinalizado(CancellationToken ct)
        {
            var p = await _db.ProcesosElectorales
                .Where(x => x.Estado == ProcesoEstado.Finalizado)
                .OrderByDescending(x => x.FinUtc)
                .ThenByDescending(x => x.CreadoUtc)
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
                return Ok(ApiResponse<ProcesoActivoDto>.Fail("No hay procesos finalizados."));

            return Ok(ApiResponse<ProcesoActivoDto>.Success(p));
        }

        [HttpGet("{procesoId:guid}/juntas-reportadas")]
        public async Task<ActionResult<ApiResponse<long>>> GetJuntasReportadas(Guid procesoId, CancellationToken ct)
        {
            var n = await _db.Juntas.CountAsync(j => j.Cerrada, ct);
            return Ok(ApiResponse<long>.Success(n));
        }
    }
}
