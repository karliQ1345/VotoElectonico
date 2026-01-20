using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Juntas;
using VotoElectonico.Juntas;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/juntas")]
    public class JuntasController : ControllerBase
    {
        public JuntasController() { }

        [HttpGet("panel")]
        public ActionResult<ApiResponse<JefePanelDto>> GetPanel()
        {
            // Obtener junta del jefe autenticado + estado proceso + boton ir a votar
            var dto = new JefePanelDto
            {
                JuntaId = "GUID",
                JuntaCodigo = "J-001",
                Provincia = "Pichincha",
                Canton = "Quito",
                Parroquia = "Centro",
                ProcesoActivo = true,
                BotonIrAVotarDisponible = true
            };

            return Ok(ApiResponse<JefePanelDto>.Success(dto));
        }

        [HttpPost("verificar-votante")]
        public ActionResult<ApiResponse<JefeVerificarVotanteResponseDto>> VerificarVotante([FromBody] JefeVerificarVotanteRequestDto req)
        {
            // Validar votante, junta, proceso activo, ya voto, etc.
            // Devolver datos + codigo unico (solo si permitido)
            var resp = new JefeVerificarVotanteResponseDto
            {
                Permitido = true,
                Mensaje = "Votante verificado correctamente",
                Votante = new VotanteVerificacionDto
                {
                    UsuarioId = "GUID",
                    Cedula = req.CedulaVotante,
                    NombreCompleto = "Juan Perez",
                    Email = "juan@gmail.com",
                    FotoUrl = "https://...",
                    Provincia = "Pichincha",
                    Canton = "Quito",
                    Parroquia = "Centro",
                    Genero = "M",
                    YaVoto = false
                },
                CodigoUnico = "123456"
            };

            return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(resp));
        }
    }
}
