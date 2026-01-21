using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Reportes;
using VotoElectonico.Models.Enums;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ReportesController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReporteResponseDto>>> GetReporte([FromBody] ReporteFiltroDto filtro, CancellationToken ct)
        {
            if (!Guid.TryParse(filtro.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<ReporteResponseDto>.Fail("ProcesoElectoralId inválido."));
            if (!Guid.TryParse(filtro.EleccionId, out var eleccionId))
                return BadRequest(ApiResponse<ReporteResponseDto>.Fail("EleccionId inválido."));

            if (!Enum.TryParse<DimensionReporte>(filtro.Dimension, out var dim))
                return BadRequest(ApiResponse<ReporteResponseDto>.Fail("Dimension inválida."));

            var items = await _db.ResultadosAgregados
                .Where(x => x.ProcesoElectoralId == procesoId && x.EleccionId == eleccionId && x.Dimension == dim)
                .Select(x => new ReporteItemDto
                {
                    DimensionValor = x.DimensionValor,
                    Opcion = x.Opcion,
                    Votos = x.Votos
                })
                .ToListAsync(ct);

            var actualizado = await _db.ResultadosAgregados
                .Where(x => x.ProcesoElectoralId == procesoId && x.EleccionId == eleccionId && x.Dimension == dim)
                .MaxAsync(x => (DateTime?)x.ActualizadoUtc, ct) ?? DateTime.UtcNow;

            var resp = new ReporteResponseDto
            {
                ProcesoElectoralId = procesoId.ToString(),
                EleccionId = eleccionId.ToString(),
                Dimension = dim.ToString(),
                Items = items,
                ActualizadoUtc = actualizado
            };

            return Ok(ApiResponse<ReporteResponseDto>.Success(resp));
        }
    }
}
