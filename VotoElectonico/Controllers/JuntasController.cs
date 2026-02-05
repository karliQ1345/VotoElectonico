using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Email;
using VotoElectonico.DTOs.Juntas;
using VotoElectonico.Juntas;
using VotoElectonico.Models;
using VotoElectonico.Models.Enums;
using VotoElectonico.Services.Email;
using VotoElectonico.Utils;

namespace VotoElectonico.Controllers
{
    [Authorize(Roles = nameof(RolTipo.JefeJunta))]
    [ApiController]
    [Route("api/juntas")]
    public class JuntasController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _email;

        public JuntasController(ApplicationDbContext db, IEmailSender email)
        {
            _db = db;
            _email = email;
        }

        // GET api/juntas/panel/{procesoId}
        [HttpGet("panel/{procesoId:guid}")]
        public async Task<ActionResult<ApiResponse<JefePanelDto>>> Panel(Guid procesoId, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(ApiResponse<JefePanelDto>.Fail("Token inválido."));

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.JefeJuntaUsuarioId == userId.Value, ct);
            if (junta == null) return NotFound(ApiResponse<JefePanelDto>.Fail("Junta no asignada al jefe."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<JefePanelDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo
                         && DateTime.UtcNow >= proceso.InicioUtc
                         && DateTime.UtcNow <= proceso.FinUtc;

            // Botón "Ir a votar" disponible si el jefe aún no votó en el padrón del proceso
            var padronJefe = await _db.PadronRegistros
                .FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == userId.Value, ct);

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

        // POST api/juntas/verificar
        [HttpPost("verificar")]
        public async Task<ActionResult<ApiResponse<JefeVerificarVotanteResponseDto>>> Verificar(
            [FromBody] JefeVerificarVotanteRequestDto req,
            CancellationToken ct)
        {
            // 0) Validaciones básicas
            if (req == null)
                return BadRequest(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Body requerido."));

            if (!Guid.TryParse(req.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("ProcesoElectoralId inválido."));

            var jefeId = GetUserId();
            if (jefeId == null)
                return Unauthorized(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Token inválido."));

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.JefeJuntaUsuarioId == jefeId.Value, ct);
            if (junta == null)
                return NotFound(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Junta no asignada."));

            if (junta.Cerrada)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Esta junta ya fue finalizada. No se pueden generar más códigos.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null)
                return NotFound(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo
                         && DateTime.UtcNow >= proceso.InicioUtc
                         && DateTime.UtcNow <= proceso.FinUtc;

            if (!activo)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Proceso electoral no está activo.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            var ced = req.CedulaVotante?.Trim();
            if (string.IsNullOrWhiteSpace(ced))
                return BadRequest(ApiResponse<JefeVerificarVotanteResponseDto>.Fail("CedulaVotante requerida."));

            // 1) Buscar votante
            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == ced && x.Activo, ct);
            if (user == null)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Votante no existe o está inactivo.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            // 2) Verificar que esté en padrón del proceso
            var padron = await _db.PadronRegistros.FirstOrDefaultAsync(x =>
                x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

            if (padron == null)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "Votante no está registrado en el padrón para este proceso.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            // 3) Verificar que pertenezca a la misma junta del jefe
            if (padron.JuntaId != junta.Id)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "El votante no pertenece a esta junta.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            // 4) Si ya votó, no entregar código
            if (padron.YaVoto)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "El votante ya registró sufragio.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            // 5) Marcar verificación presencial
            if (padron.VerificadoUtc == null)
            {
                padron.VerificadoUtc = DateTime.UtcNow;
                padron.VerificadoPorJefeId = jefeId.Value;
            }

            // 6) Opción 1: Generar código real al verificar y guardarlo como hash
            var codigo = await _db.CodigosVotacion.FirstOrDefaultAsync(x =>
                x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

            // Si el código no existe, lo creamos
            // Si existe pero ya fue usado, bloqueamos
            // Si existe y no fue usado, regeneramos (para que el jefe pueda decirlo de nuevo)
            if (codigo != null && codigo.Usado)
            {
                return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(new JefeVerificarVotanteResponseDto
                {
                    Permitido = false,
                    Mensaje = "El código de votación ya fue usado.",
                    Votante = null,
                    CodigoUnico = null
                }));
            }

            // Generar un código plano (solo lo veremos aquí, luego solo queda el hash en BD)
            var codigoPlano = SecurityHelpers.GenerateNumericCode(6);

            if (codigo == null)
            {
                codigo = new CodigoVotacion
                {
                    Id = Guid.NewGuid(),
                    ProcesoElectoralId = procesoId,
                    UsuarioId = user.Id,
                    CodigoHash = SecurityHelpers.HashWithSalt(codigoPlano),
                    Usado = false,
                    UsadoUtc = null,
                    CreadoUtc = DateTime.UtcNow,
                    MostradoAlJefeUtc = DateTime.UtcNow
                };
                _db.CodigosVotacion.Add(codigo);
            }
            else
            {
                // Regenerar para reentregar (útil en pruebas y si el jefe necesita repetirlo)
                codigo.CodigoHash = SecurityHelpers.HashWithSalt(codigoPlano);
                codigo.MostradoAlJefeUtc = DateTime.UtcNow;
                codigo.Usado = false;
                codigo.UsadoUtc = null;
            }
            await _db.SaveChangesAsync(ct);
            // 7) Respuesta
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
                CodigoUnico = codigoPlano
            };

            return Ok(ApiResponse<JefeVerificarVotanteResponseDto>.Success(resp));
        }

        [HttpPost("finalizar")]
        public async Task<ActionResult<ApiResponse<FinalizarJuntaResponseDto>>> Finalizar(
            [FromBody] FinalizarJuntaRequestDto req,
            CancellationToken ct)
        {
            if (!Guid.TryParse(req?.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<FinalizarJuntaResponseDto>.Fail("ProcesoElectoralId inválido."));

            var jefeId = GetUserId();
            if (jefeId == null)
                return Unauthorized(ApiResponse<FinalizarJuntaResponseDto>.Fail("Token inválido."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null)
                return NotFound(ApiResponse<FinalizarJuntaResponseDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo
                         && DateTime.UtcNow >= proceso.InicioUtc
                         && DateTime.UtcNow <= proceso.FinUtc;

            if (!activo)
                return BadRequest(ApiResponse<FinalizarJuntaResponseDto>.Fail("Proceso no está activo."));

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.JefeJuntaUsuarioId == jefeId.Value, ct);
            if (junta == null)
                return NotFound(ApiResponse<FinalizarJuntaResponseDto>.Fail("Junta no asignada."));

            if (junta.Cerrada)
            {
                return Ok(ApiResponse<FinalizarJuntaResponseDto>.Success(new FinalizarJuntaResponseDto
                {
                    Ok = true,
                    Mensaje = "La junta ya estaba finalizada.",
                    JuntaCodigo = junta.Codigo,
                    CerradaUtc = junta.CerradaUtc
                }));
            }

            junta.Cerrada = true;
            junta.CerradaUtc = DateTime.UtcNow;
            junta.CerradaPorJefeId = jefeId.Value;

            await _db.SaveChangesAsync(ct);
            var admins = await _db.UsuarioRoles
                .Where(r => r.Rol == RolTipo.Administrador)
                .Select(r => r.Usuario.Email)
                .Distinct()
                .ToListAsync(ct);

            var subject = $"Junta {junta.Codigo} finalizó su votación";

            var html = $@"
                <div style=""font-family:Arial,sans-serif;max-width:640px;margin:auto"">
                <h2>Junta finalizada</h2>
                <p>La junta <b>{junta.Codigo}</b> finalizó su votación.</p>

                <div style=""border:1px solid #eee;border-radius:12px;padding:14px;background:#fafafa"">
                <p><b>Proceso:</b> {proceso.Nombre}</p>
                <p><b>Junta:</b> {junta.Codigo}</p>
                <p><b>Provincia:</b> {junta.Provincia}</p>
                <p><b>Cantón:</b> {junta.Canton}</p>
                <p><b>Parroquia:</b> {junta.Parroquia}</p>
                <p><b>Fecha (UTC):</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                </div>

                <p style=""font-size:12px;color:#666"">Mensaje automático del sistema de voto.</p>
                </div>";

            foreach (var mail in admins)
            {
                if (string.IsNullOrWhiteSpace(mail)) continue;

                await _email.SendAsync(new SendEmailDto
                {
                    ToEmail = mail,
                    ToName = "Administrador",
                    Subject = subject,
                    HtmlContent = html
                }, ct);
            }
            return Ok(ApiResponse<FinalizarJuntaResponseDto>.Success(new FinalizarJuntaResponseDto
            {
                Ok = true,
                Mensaje = "Junta finalizada. Ya no se permitirán más votaciones en esta junta.",
                JuntaCodigo = junta.Codigo,
                CerradaUtc = junta.CerradaUtc
            }));
        }
    }
}
