using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Email;
using VotoElectonico.DTOs.Votacion;
using VotoElectonico.Models;
using VotoElectonico.Models.Enums;
using VotoElectonico.Utils;
using VotoElectonico.Services.Email;

namespace VotoElectonico.Controllers
{
    [Authorize] // cualquier usuario autenticado puede consumir (pero el voto valida cédula/código)
    [ApiController]
    [Route("api/votacion")]
    public class VotacionController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _email;
        private readonly IConfiguration _cfg;

        public VotacionController(ApplicationDbContext db, IEmailSender email, IConfiguration cfg)
        {
            _db = db;
            _email = email;
            _cfg = cfg;
        }

        [AllowAnonymous]
        [HttpPost("iniciar")]
        public async Task<ActionResult<ApiResponse<IniciarVotacionResponseDto>>> Iniciar([FromBody] IniciarVotacionRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<IniciarVotacionResponseDto>.Fail("ProcesoElectoralId inválido."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<IniciarVotacionResponseDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo && DateTime.UtcNow >= proceso.InicioUtc && DateTime.UtcNow <= proceso.FinUtc;
            if (!activo)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto
                {
                    Habilitado = false,
                    Mensaje = "Proceso electoral no activo."
                }));

            var ced = req.Cedula?.Trim();
            var code = req.CodigoUnico?.Trim();
            if (string.IsNullOrWhiteSpace(ced) || string.IsNullOrWhiteSpace(code))
                return BadRequest(ApiResponse<IniciarVotacionResponseDto>.Fail("Cedula y CodigoUnico requeridos."));

            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == ced && x.Activo, ct);
            if (user == null)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto { Habilitado = false, Mensaje = "Usuario no válido." }));

            var padron = await _db.PadronRegistros.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
            if (padron == null)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto { Habilitado = false, Mensaje = "No consta en padrón." }));

            if (padron.YaVoto)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto { Habilitado = false, Mensaje = "Ya votó." }));

            var cod = await _db.CodigosVotacion.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
            if (cod == null)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto { Habilitado = false, Mensaje = "Código no asignado." }));

            if (cod.Usado)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto { Habilitado = false, Mensaje = "Código ya usado." }));

            if (!SecurityHelpers.VerifyHashWithSalt(code, cod.CodigoHash))
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto { Habilitado = false, Mensaje = "Código incorrecto." }));

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.Id == padron.JuntaId, ct);

            return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto
            {
                Habilitado = true,
                Mensaje = "Votación habilitada.",
                JuntaCodigo = junta?.Codigo ?? "",
                Recinto = junta?.Recinto
            }));
        }

        [AllowAnonymous]
        [HttpGet("boleta")]
        public async Task<ActionResult<ApiResponse<BoletaDataDto>>> Boleta([FromQuery] Guid procesoId, [FromQuery] Guid eleccionId, CancellationToken ct)
        {
            var eleccion = await _db.Elecciones
                .Include(x => x.Listas)
                .Include(x => x.Candidatos)
                .FirstOrDefaultAsync(x => x.Id == eleccionId && x.ProcesoElectoralId == procesoId && x.Activa, ct);

            if (eleccion == null) return NotFound(ApiResponse<BoletaDataDto>.Fail("Elección no existe o no activa."));

            var dto = new BoletaDataDto
            {
                ProcesoElectoralId = procesoId.ToString(),
                EleccionId = eleccion.Id.ToString(),
                TipoEleccion = eleccion.Tipo.ToString(),
                Titulo = eleccion.Titulo,
                MaxSeleccionIndividual = eleccion.MaxSeleccionIndividual
            };

            dto.Listas = eleccion.Listas.Select(l => new BoletaListaDto
            {
                PartidoListaId = l.Id.ToString(),
                Nombre = l.Nombre,
                Codigo = l.Codigo,
                LogoUrl = l.LogoUrl
            }).ToList();

            dto.Candidatos = eleccion.Candidatos
                .Where(c => c.Activo)
                .Select(c => new BoletaCandidatoDto
                {
                    CandidatoId = c.Id.ToString(),
                    NombreCompleto = c.NombreCompleto,
                    Cargo = c.Cargo,
                    FotoUrl = c.FotoUrl,
                    PartidoListaId = c.PartidoListaId?.ToString()
                })
                .ToList();

            return Ok(ApiResponse<BoletaDataDto>.Success(dto));
        }

        [AllowAnonymous]
        [HttpPost("emitir")]
        public async Task<ActionResult<ApiResponse<EmitirVotoResponseDto>>> Emitir([FromBody] EmitirVotoRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("ProcesoElectoralId inválido."));
            if (!Guid.TryParse(req.EleccionId, out var eleccionId))
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("EleccionId inválido."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<EmitirVotoResponseDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo && DateTime.UtcNow >= proceso.InicioUtc && DateTime.UtcNow <= proceso.FinUtc;
            if (!activo) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Proceso no activo."));

            var eleccion = await _db.Elecciones.FirstOrDefaultAsync(x => x.Id == eleccionId && x.ProcesoElectoralId == procesoId && x.Activa, ct);
            if (eleccion == null) return NotFound(ApiResponse<EmitirVotoResponseDto>.Fail("Elección no válida."));

            var ced = req.Cedula?.Trim();
            var code = req.CodigoUnico?.Trim();
            if (string.IsNullOrWhiteSpace(ced) || string.IsNullOrWhiteSpace(code))
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Cedula y CodigoUnico requeridos."));

            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == ced && x.Activo, ct);
            if (user == null) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Usuario no válido."));

            var padron = await _db.PadronRegistros.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
            if (padron == null) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("No consta en padrón."));
            if (padron.YaVoto) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Ya votó."));

            var cod = await _db.CodigosVotacion.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
            if (cod == null) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Código no asignado."));
            if (cod.Usado) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Código ya usado."));
            if (!SecurityHelpers.VerifyHashWithSalt(code, cod.CodigoHash))
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Código incorrecto."));

            // Validación elección
            object votoPayload;

            if (eleccion.Tipo == EleccionTipo.Presidente_SiNoBlanco)
            {
                var op = (req.OpcionPresidente ?? "").Trim().ToUpperInvariant();
                if (op != "SI" && op != "NO" && op != "BLANCO")
                    return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Opción Presidente inválida (SI/NO/BLANCO)."));

                votoPayload = new { tipo = "Presidente", opcion = op };
            }
            else // Asambleistas
            {
                var listaId = req.PartidoListaId?.Trim();
                var candIds = req.CandidatoIds;

                var votoPlancha = !string.IsNullOrWhiteSpace(listaId);
                var votoIndividual = candIds != null && candIds.Count > 0;

                if (votoPlancha == votoIndividual)
                    return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Debe elegir plancha o candidatos individuales (solo uno)."));

                if (votoPlancha)
                {
                    if (!Guid.TryParse(listaId!, out var lid))
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("PartidoListaId inválido."));
                    var existe = await _db.PartidosListas.AnyAsync(x => x.Id == lid && x.EleccionId == eleccionId, ct);
                    if (!existe) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Lista no existe."));
                    votoPayload = new { tipo = "Asambleistas", modo = "Plancha", partidoListaId = lid };
                }
                else
                {
                    var max = eleccion.MaxSeleccionIndividual ?? 0;
                    if (max <= 0) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Elección sin límite configurado."));
                    if (candIds!.Count > max) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail($"Máximo permitido: {max}."));

                    var parsed = new List<Guid>();
                    foreach (var s in candIds)
                    {
                        if (!Guid.TryParse(s, out var gid))
                            return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("CandidatoId inválido."));
                        parsed.Add(gid);
                    }

                    var validos = await _db.Candidatos.CountAsync(x => x.EleccionId == eleccionId && x.Activo && parsed.Contains(x.Id), ct);
                    if (validos != parsed.Count)
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Uno o más candidatos no existen o no pertenecen a esta elección."));

                    votoPayload = new { tipo = "Asambleistas", modo = "Individual", candidatoIds = parsed };
                }
            }

            // Cifrado anónimo
            var keyB64 = _cfg["Crypto:KeyBase64"];
            var keyVer = _cfg["Crypto:KeyVersion"] ?? "v1";
            if (string.IsNullOrWhiteSpace(keyB64))
                return StatusCode(500, ApiResponse<EmitirVotoResponseDto>.Fail("Crypto:KeyBase64 no configurado."));

            var key = Convert.FromBase64String(keyB64);

            var (cipherB64, nonceB64, tagB64) = SecurityHelpers.EncryptAesGcm(votoPayload, key);

            // Junta
            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.Id == padron.JuntaId, ct);
            if (junta == null) return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Junta no existe."));

            // Guardar voto anónimo
            var voto = new VotoAnonimo
            {
                Id = Guid.NewGuid(),
                ProcesoElectoralId = procesoId,
                EleccionId = eleccionId,
                JuntaId = junta.Id,
                EmitidoUtc = DateTime.UtcNow,
                CipherTextBase64 = cipherB64,
                NonceBase64 = nonceB64,
                TagBase64 = tagB64,
                KeyVersion = keyVer
            };

            _db.VotosAnonimos.Add(voto);

            // Marcar sufragio + código usado
            padron.YaVoto = true;
            padron.VotoUtc = DateTime.UtcNow;

            cod.Usado = true;
            cod.UsadoUtc = DateTime.UtcNow;

            // Comprobante (con identidad)
            var comprobante = new ComprobanteVoto
            {
                Id = Guid.NewGuid(),
                ProcesoElectoralId = procesoId,
                EleccionId = eleccionId,
                UsuarioId = user.Id,
                JuntaId = junta.Id,
                JefeJuntaUsuarioId = junta.JefeJuntaUsuarioId,
                GeneradoUtc = DateTime.UtcNow,
                EstadoEnvio = ComprobanteEstado.Pendiente
            };

            _db.ComprobantesVoto.Add(comprobante);

            await _db.SaveChangesAsync(ct);

            // Enviar “papeleta” por correo (aquí como HTML simple; PDF lo puedes agregar luego)
            var emailMasked = SecurityHelpers.MaskEmail(user.Email);
            var html = $@"
            <h3>Comprobante de Votación</h3>
            <p>Su papeleta ha sido registrada correctamente.</p>
            <p><b>Proceso:</b> {proceso.Nombre}</p>
            <p><b>Elección:</b> {eleccion.Titulo}</p>
            <p><b>Junta:</b> {junta.Codigo}</p>
            <p><b>Jefe de Junta:</b> {junta.JefeJuntaUsuarioId}</p>
            <p><i>Este correo es un comprobante de participación.</i></p>";

            var send = new SendEmailDto
            {
                ToEmail = user.Email,
                ToName = user.NombreCompleto,
                Subject = "Papeleta / Comprobante de Votación",
                HtmlContent = html
            };

            var (sent, messageId, error) = await _email.SendAsync(send, ct);

            comprobante.BrevoMessageId = messageId;
            if (sent)
            {
                comprobante.EstadoEnvio = ComprobanteEstado.Enviado;
                comprobante.EnviadoUtc = DateTime.UtcNow;
            }
            else
            {
                comprobante.EstadoEnvio = ComprobanteEstado.Fallido;
                comprobante.ErrorEnvio = error;
            }

            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<EmitirVotoResponseDto>.Success(new EmitirVotoResponseDto
            {
                Ok = true,
                Mensaje = "Voto registrado.",
                PapeletaEnviada = sent,
                EmailEnmascarado = emailMasked
            }));
        }
    }
}
