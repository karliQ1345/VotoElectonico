using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Votacion;

namespace VotoElectonico.Controllers
{
    [ApiController]
    [Route("api/votacion")]
    public class VotacionController : ControllerBase
    {
        public VotacionController() { }

        [HttpPost("iniciar")]
        public ActionResult<ApiResponse<IniciarVotacionResponseDto>> Iniciar([FromBody] IniciarVotacionRequestDto req)
        {
            // Validar codigo unico, no usado, padron, no ha votado, proceso activo
            var resp = new IniciarVotacionResponseDto
            {
                Habilitado = true,
                Mensaje = "Puede votar",
                JuntaCodigo = "J-001",
                Recinto = "Escuela X"
            };
            return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(resp));
        }

        [HttpGet("boleta")]
        public ActionResult<ApiResponse<BoletaDataDto>> GetBoleta([FromQuery] string procesoElectoralId, [FromQuery] string eleccionId)
        {
            // Devolver listas/candidatos según elección
            var dto = new BoletaDataDto
            {
                ProcesoElectoralId = procesoElectoralId,
                EleccionId = eleccionId,
                TipoEleccion = "Presidente_SiNoBlanco",
                Titulo = "Elección Presidencial"
            };

            return Ok(ApiResponse<BoletaDataDto>.Success(dto));
        }

        [HttpPost("emitir")]
        public ActionResult<ApiResponse<EmitirVotoResponseDto>> Emitir([FromBody] EmitirVotoRequestDto req)
        {
            // Cifrar voto, guardar voto anonimo, marcar padron ya voto, marcar codigo usado
            // Generar papeleta PDF y enviar por Brevo
            var resp = new EmitirVotoResponseDto
            {
                Ok = true,
                Mensaje = "Voto registrado. Su papeleta ha sido enviada a su correo.",
                PapeletaEnviada = true,
                EmailEnmascarado = "jua****@gmail.com"
            };

            return Ok(ApiResponse<EmitirVotoResponseDto>.Success(resp));
        }
    }
}
