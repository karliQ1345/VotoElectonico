using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Procesos;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/procesos")]
    public class ProcesosController : ControllerBase
    {
        public ProcesosController() { }

        [HttpPost]
        public ActionResult<ApiResponse<IdResponseDto>> Crear([FromBody] CrearProcesoRequestDto req)
        {
            // Crear proceso
            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = "GUID" }, "Proceso creado"));
        }

        [HttpGet]
        public ActionResult<ApiResponse<List<ProcesoResumenDto>>> Listar()
        {
            // TODO: listar procesos
            return Ok(ApiResponse<List<ProcesoResumenDto>>.Success(new List<ProcesoResumenDto>()));
        }

        [HttpPost("{procesoId}/activar")]
        public IActionResult Activar(string procesoId)
        {
            // TODO: cambiar estado a Activo
            return Ok();
        }

        [HttpPost("{procesoId}/finalizar")]
        public IActionResult Finalizar(string procesoId)
        {
            // TODO: cambiar estado a Finalizado
            return Ok();
        }
    }
}
