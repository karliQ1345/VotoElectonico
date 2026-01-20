using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Elecciones;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/elecciones")]
    public class EleccionesController : ControllerBase
    {
        public EleccionesController() { }

        [HttpPost]
        public ActionResult<ApiResponse<IdResponseDto>> CrearEleccion([FromBody] CrearEleccionRequestDto req)
        {
            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = "GUID" }, "Elección creada"));
        }

        [HttpPost("listas")]
        public ActionResult<ApiResponse<IdResponseDto>> CrearLista([FromBody] CrearListaRequestDto req)
        {
            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = "GUID" }, "Lista creada"));
        }

        [HttpPost("candidatos")]
        public ActionResult<ApiResponse<IdResponseDto>> CrearCandidato([FromBody] CrearCandidatoRequestDto req)
        {
            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = "GUID" }, "Candidato creado"));
        }
    }
