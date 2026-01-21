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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<IdResponseDto>>> Crear([FromBody] CrearProcesoRequestDto req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Nombre))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("Nombre requerido."));
            if (req.FinUtc <= req.InicioUtc)
                return BadRequest(ApiResponse<IdResponseDto>.Fail("FinUtc debe ser mayor a InicioUtc."));

            var p = new ProcesoElectoral
            {
                Id = Guid.NewGuid(),
                Nombre = req.Nombre.Trim(),
                InicioUtc = req.InicioUtc,
                FinUtc = req.FinUtc,
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
                    FinUtc = x.FinUtc
                })
                .ToListAsync(ct);

            return Ok(ApiResponse<List<ProcesoResumenDto>>.Success(list));
        }

        [HttpPost("{procesoId:guid}/activar")]
        public async Task<ActionResult<ApiResponse<string>>> Activar(Guid procesoId, CancellationToken ct)
        {
            var p = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (p == null) return NotFound(ApiResponse<string>.Fail("Proceso no existe."));

            if (p.Estado == ProcesoEstado.Finalizado)
                return BadRequest(ApiResponse<string>.Fail("No se puede activar un proceso finalizado."));

            p.Estado = ProcesoEstado.Activo;
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<string>.Success("OK", "Proceso activado."));
        }

        [HttpPost("{procesoId:guid}/finalizar")]
        public async Task<ActionResult<ApiResponse<string>>> Finalizar(Guid procesoId, CancellationToken ct)
        {
            var p = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (p == null) return NotFound(ApiResponse<string>.Fail("Proceso no existe."));

            p.Estado = ProcesoEstado.Finalizado;
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<string>.Success("OK", "Proceso finalizado."));
        }
    }
}
