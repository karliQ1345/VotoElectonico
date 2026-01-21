using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Juntas;
using VotoElectonico.Juntas;
using VotoElectonico.Models.Enums;

namespace VotoElectonico.Controllers
{
    [Authorize(Roles = nameof(RolTipo.JefeJunta))]
    [ApiController]
    [Route("api/juntas")]
    public class JuntasController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        public JuntasController(ApplicationDbContext db) => _db = db;

        [HttpGet("panel/{procesoId:guid}")]
        public async Task<ActionResult<ApiResponse<JefePanelDto>>> Panel(Guid procesoId, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(ApiResponse<JefePanelDto>.Fail("Token inválido."));

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.JefeJuntaUsuarioId == userId.Value, ct);
            if (junta == null) return NotFound(ApiResponse<JefePanelDto>.Fail("Junta no asignada al jefe."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<JefePanelDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo && DateTime.UtcNow >= proceso.InicioUtc && DateTime.UtcNow <= proceso.FinUtc;

            // botón "Ir a votar" disponible si el jefe aún no votó en padrón
            var padronJefe = await _db.PadronRegistros.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == userId.Value, ct);
            var boton = padronJefe != null && !padronJefe.YaVoto;

            var dto = new JefePanelDto
            {
                JuntaId = junta.Id.ToString(),
                JuntaCodigo = junta.Codigo,
                Provincia = junta.Provincia,
                Canton = junta.Canton,
                Parroquia = junta.Parroquia,
                ProcesoActivo = activo,
                BotonIrAVotarDisponible = boton
            };

            return Ok(ApiResponse<JefePanelDto>.Success(dto));
        }

        [HttpPost("verificar")]
        public async Task<ActionResult<ApiResponse<JefeVerificarVotanteResponseDto>>> Verificar([FromBody] JefeVerificarVotanteRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("ProcesoElectoralId inválido."));

            var jefeId = GetUserId();
            if (jefeId == null) return Unauthorized(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Token inválido."));

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.JefeJuntaUsuarioId == jefeId.Value, ct);
            if (junta == null) return NotFound(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Junta no asignada."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo && DateTime.UtcNow >= proceso.InicioUtc && DateTime.UtcNow <= proceso.FinUtc;
            if (!activo)
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Proceso electoral no está activo."
                }));

            var ced = req.CedulaVotante?.Trim();
            if (string.IsNullOrWhiteSpace(ced))
                return BadRequest(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("CedulaVotante requerida."));

            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == ced && x.Activo, ct);
            if (user == null)
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Votante no existe o está inactivo."
                }));

            var padron = await _db.PadronRegistros.FirstOrDefaultAsync(x =>
                x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

            if (padron == null)
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Votante no está registrado en el padrón para este proceso."
                }));

            if (padron.JuntaId != junta.Id)
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "El votante no pertenece a esta junta."
                }));

            // Marca verificación
            if (padron.VerificadoUtc == null)
            {
                padron.VerificadoUtc = DateTime.UtcNow;
                padron.VerificadoPorJefeId = jefeId.Value;
            }

            // Obtener código (no plano). Solo devolvemos un "placeholder" de entrega (si ya existía, no se puede recuperar el texto plano)
            // En tu diseño, el código se genera desde el padrón; aquí asumimos que YA existe y que el votante lo ingresa.
            var codigo = await _db.CodigosVotacion.FirstOrDefaultAsync(x =>
                x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

            if (codigo == null)
            {
                // si por algún motivo no existe, bloqueamos
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "No existe código de votación asignado."
                }));
            }

            codigo.MostradoAlJefeUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var resp = new JefeVerificarVotanteResponseDto
            {
                Permitido = true,
                Mensaje = "Votante verificado.",
                Votante = new VotanteVerificacionDto
                {
                    UsuarioId = user.Id.ToString(),
                    Cedula = user.Cedula,
                    NombreCompleto = user.NombreCompleto,
                    Email = user.Email,
                    Telefono = user.Telefono,
                    FotoUrl = user.FotoUrl,
                    Provincia = user.Provincia ?? "",
                    Canton = user.Canton ?? "",
                    Parroquia = user.Parroquia ?? "",
                    Genero = user.Genero ?? "",
                    YaVoto = padron.YaVoto
                },
                // ⚠️ Importante: no puedes devolver el código REAL si solo guardas hash.
                CodigoUnico = "ASIGNADO_EN_BD (hash)" // el código real lo tiene el votante; aquí solo confirmas existencia
            };

            return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(resp));
        }
    }
}
