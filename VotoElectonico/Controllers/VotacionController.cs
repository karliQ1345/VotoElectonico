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
            if (junta == null)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto
                {
                    Habilitado = false,
                    Mensaje = "Junta no existe."
                }));

            if (junta.Cerrada)
                return Ok(ApiResponse<IniciarVotacionResponseDto>.Success(new IniciarVotacionResponseDto
                {
                    Habilitado = false,
                    Mensaje = "La junta ya fue finalizada."
                }));

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
            return Ok(ApiResponse<BoletaDataDto>.Success(BuildBoletaDto(eleccion, procesoId)));
        }

        [AllowAnonymous]
        [HttpGet("boleta-activa")]
        public async Task<ActionResult<ApiResponse<BoletaDataDto>>> BoletaActiva([FromQuery] Guid procesoId, CancellationToken ct)
        {
            var eleccion = await _db.Elecciones
                .Include(x => x.Listas)
                .Include(x => x.Candidatos)
                .FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.Activa, ct);

            if (eleccion == null) return NotFound(ApiResponse<BoletaDataDto>.Fail("No hay elección activa para este proceso."));

            return Ok(ApiResponse<BoletaDataDto>.Success(BuildBoletaDto(eleccion, procesoId)));
        }

        private static BoletaDataDto BuildBoletaDto(Eleccion eleccion, Guid procesoId)
        {

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

            return dto;
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
            if (proceso == null)
                return NotFound(ApiResponse<EmitirVotoResponseDto>.Fail("Proceso no existe."));

            var activo = proceso.Estado == ProcesoEstado.Activo
                         && DateTime.UtcNow >= proceso.InicioUtc
                         && DateTime.UtcNow <= proceso.FinUtc;

            if (!activo)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Proceso no activo."));

            var eleccion = await _db.Elecciones
                .FirstOrDefaultAsync(x => x.Id == eleccionId && x.ProcesoElectoralId == procesoId && x.Activa, ct);

            if (eleccion == null)
                return NotFound(ApiResponse<EmitirVotoResponseDto>.Fail("Elección no válida."));

            var ced = req.Cedula?.Trim();
            var code = req.CodigoUnico?.Trim();

            if (string.IsNullOrWhiteSpace(ced) || string.IsNullOrWhiteSpace(code))
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Cedula y CodigoUnico requeridos."));

            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == ced && x.Activo, ct);
            if (user == null)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Usuario no válido."));

            var padron = await _db.PadronRegistros.FirstOrDefaultAsync(
                x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

            if (padron == null)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("No consta en padrón."));
            if (padron.YaVoto)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Ya votó."));

            var cod = await _db.CodigosVotacion.FirstOrDefaultAsync(
                x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

            if (cod == null)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Código no asignado."));
            if (cod.Usado)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Código ya usado."));
            if (!SecurityHelpers.VerifyHashWithSalt(code, cod.CodigoHash))
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Código incorrecto."));

            // =========================
            // VALIDACIÓN DE ELECCIÓN
            // =========================
            object votoPayload;

            if (eleccion.Tipo == EleccionTipo.Nominal)
            {
                // NUEVO: votar por candidato (GUID) o BLANCO
                var raw = (req.PresidenteCandidatoId ?? "").Trim();

                if (!string.IsNullOrWhiteSpace(raw))
                {
                    if (raw.Equals("BLANCO", StringComparison.OrdinalIgnoreCase))
                    {
                        votoPayload = new { tipo = "Presidente", modo = "Blanco" };
                    }
                    else
                    {
                        if (!Guid.TryParse(raw, out var candidatoId))
                            return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("PresidenteCandidatoId inválido."));

                        var existeCandidato = await _db.Candidatos.AnyAsync(x =>
                            x.Id == candidatoId &&
                            x.EleccionId == eleccionId &&
                            x.Activo, ct);

                        if (!existeCandidato)
                            return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Candidato no existe o no pertenece a esta elección."));

                        votoPayload = new { tipo = "Presidente", modo = "Candidato", candidatoId };
                    }
                }
                else
                {
                    // FALLBACK OPCIONAL: seguir aceptando SI/NO/BLANCO (si tu MVC viejo todavía lo manda)
                    var op = (req.OpcionPresidente ?? "").Trim().ToUpperInvariant();
                    if (op != "SI" && op != "NO" && op != "BLANCO")
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Opción Presidente inválida (SI/NO/BLANCO)."));

                    votoPayload = new { tipo = "Presidente", modo = "SiNoBlanco", opcion = op };
                }
            }
            else // Plurinominal
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

                    var existeLista = await _db.PartidosListas.AnyAsync(x => x.Id == lid && x.EleccionId == eleccionId, ct);
                    if (!existeLista)
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Lista no existe."));

                    votoPayload = new { tipo = "Plurinominal", modo = "Plancha", partidoListaId = lid };
                }
                else
                {
                    var max = eleccion.MaxSeleccionIndividual ?? 0;
                    if (max <= 0)
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Elección sin límite configurado."));
                    if (candIds!.Count > max)
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail($"Máximo permitido: {max}."));

                    var parsed = new List<Guid>();
                    foreach (var s in candIds)
                    {
                        if (!Guid.TryParse(s, out var gid))
                            return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("CandidatoId inválido."));
                        parsed.Add(gid);
                    }

                    var validos = await _db.Candidatos.CountAsync(x =>
                        x.EleccionId == eleccionId && x.Activo && parsed.Contains(x.Id), ct);

                    if (validos != parsed.Count)
                        return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Uno o más candidatos no existen o no pertenecen a esta elección."));

                    votoPayload = new { tipo = "Plurinominal", modo = "Individual", candidatoIds = parsed };
                }
            }

            var keyB64 = _cfg["Crypto:KeyBase64"];
            var keyVer = _cfg["Crypto:KeyVersion"] ?? "v1";
            if (string.IsNullOrWhiteSpace(keyB64))
                return StatusCode(500, ApiResponse<EmitirVotoResponseDto>.Fail("Crypto:KeyBase64 no configurado."));

            var key = Convert.FromBase64String(keyB64);
            var (cipherB64, nonceB64, tagB64) = SecurityHelpers.EncryptAesGcm(votoPayload, key);

            var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.Id == padron.JuntaId, ct);
            if (junta == null)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("Junta no existe."));

            if (junta.Cerrada)
                return BadRequest(ApiResponse<EmitirVotoResponseDto>.Fail("La junta ya fue finalizada. No se puede emitir voto."));

            string opcion = "N/D";

            if (eleccion.Tipo == EleccionTipo.Nominal)
            {
                var raw = (req.PresidenteCandidatoId ?? "").Trim();

                if (!string.IsNullOrWhiteSpace(raw))
                {
                    opcion = raw.Equals("BLANCO", StringComparison.OrdinalIgnoreCase) ? "BLANCO" : raw; // GUID string
                }
                else
                {
                    var op = (req.OpcionPresidente ?? "").Trim().ToUpperInvariant();
                    opcion = (op == "SI" || op == "NO" || op == "BLANCO") ? op : "N/D";
                }
            }
            else
            {
                var listaId = (req.PartidoListaId ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(listaId)) opcion = listaId;     // GUID lista
                else opcion = "INDIVIDUAL";
            }

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                await IncrementarResultadoAsync(procesoId, eleccionId, DimensionReporte.Nacional, "Nacional", opcion, ct);
                await IncrementarResultadoAsync(procesoId, eleccionId, DimensionReporte.Provincia, junta.Provincia, opcion, ct);
                await IncrementarResultadoAsync(procesoId, eleccionId, DimensionReporte.Canton, junta.Canton, opcion, ct);
                await IncrementarResultadoAsync(procesoId, eleccionId, DimensionReporte.Parroquia, junta.Parroquia ?? "SIN_PARROQUIA", opcion, ct);

                var genero = (user.Genero ?? "NO_ESPECIFICA").Trim();
                await IncrementarResultadoAsync(procesoId, eleccionId, DimensionReporte.Genero, genero, opcion, ct);

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
                    EstadoEnvio = ComprobanteEstado.Pendiente,
                    PublicToken = CreatePublicToken(),
                    PublicTokenExpiraUtc = DateTime.UtcNow.AddHours(24)
                };
                _db.ComprobantesVoto.Add(comprobante);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var apiBase = (_cfg["Api:BaseUrl"] ?? "").Trim().TrimEnd('/');
                string? papeletaUrl = null;

                if (!string.IsNullOrWhiteSpace(apiBase) && !string.IsNullOrWhiteSpace(comprobante.PublicToken))
                {
                    papeletaUrl = $"{apiBase}/api/votacion/comprobante/{comprobante.PublicToken}";
                }

                Console.WriteLine($"[Emitir] ApiBase={apiBase}");
                Console.WriteLine($"[Emitir] PapeletaUrl={papeletaUrl}");

                var emailMasked = SecurityHelpers.MaskEmail(user.Email);

                var fotoUrl = (user.FotoUrl ?? "").Trim();
                var fotoHtml = "";

                if (!string.IsNullOrWhiteSpace(fotoUrl))
                {
                    fotoHtml = $@"
<p style=""margin-top:12px"">
  <img src=""{fotoUrl}"" alt=""Foto"" style=""width:140px;height:140px;object-fit:cover;border-radius:10px;border:1px solid #ddd""/>
</p>";
                }

                var btn = !string.IsNullOrWhiteSpace(papeletaUrl)
                    ? $@"
<p style=""margin-top:16px;"">
  <a href=""{papeletaUrl}""
     style=""display:inline-block;padding:12px 16px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:8px;"">
     Ver papeleta / comprobante
  </a>
</p>
<p style=""font-size:12px;color:#666"">
  Si el botón no funciona, copia y pega este enlace:<br>
  <a href=""{papeletaUrl}"">{papeletaUrl}</a>
</p>"
                    : @"<p style=""color:#b00"">No se pudo generar el enlace público (configura Api:BaseUrl / Api__BaseUrl).</p>";

                var infoCNE = @"
<div style=""margin-top:20px;padding:15px;background-color:#f8f9fa;border:1px solid #dee2e6;border-radius:8px;"">
  <p style=""margin:0;color:#333;""><strong>Nota:</strong> Este es su comprobante oficial de votación electrónica.</p>
  <p style=""margin:10px 0 0 0;font-size:12px;color:#666;"">
    Si tiene problemas para visualizar esta información, puede acercarse a las oficinas del 
    <strong>Consejo Nacional Electoral (CNE)</strong> o la entidad organizadora para obtener su certificado físico.
  </p>
</div>";

                var html = $@"
<div style=""font-family:Arial,sans-serif;max-width:640px;margin:auto"">
  <h2>Comprobante de Votación</h2>
  <p>Su voto ha sido registrado correctamente.</p>

  <div style=""border:1px solid #eee;border-radius:12px;padding:16px;background:#fafafa"">
    <p><b>Proceso:</b> {proceso.Nombre}</p>
    <p><b>Elección:</b> {eleccion.Titulo}</p>
    <p><b>Junta:</b> {junta.Codigo}</p>
    <p><b>Fecha:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>

    <hr style=""border:none;border-top:1px solid #eee;margin:12px 0"">
    <p><b>Votante:</b> {user.NombreCompleto}</p>
    <p><b>Cédula:</b> {user.Cedula}</p>
    {fotoHtml}
  </div>

  {btn}

  {infoCNE}

  <p style=""font-size:12px;color:#666;margin-top:20px;"">
    Este comprobante confirma participación. No incluye la selección del voto.
  </p>
</div>";
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
                    EmailEnmascarado = emailMasked,
                    PapeletaUrl = papeletaUrl
                }));
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }  
        }



        [AllowAnonymous]
        [HttpGet("comprobante/{token}")]
        public async Task<IActionResult> ComprobantePublico([FromRoute] string token, CancellationToken ct)
        {
            try
            {
                // 1. Validación de entrada
                token = (token ?? "").Trim();
                if (string.IsNullOrWhiteSpace(token))
                    return Content("Token inválido.", "text/plain");

                // 2. Consulta del comprobante (Sin rastreo para mejorar rendimiento en Render)
                var comp = await _db.ComprobantesVoto
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PublicToken == token, ct);

                if (comp == null)
                    return Content("El comprobante no existe o el enlace ha expirado.", "text/plain");

                // 3. Carga de datos relacionados de forma independiente (Evita problemas de pooling)
                var user = await _db.Usuarios.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == comp.UsuarioId, ct);

                var proceso = await _db.ProcesosElectorales.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == comp.ProcesoElectoralId, ct);

                var eleccion = await _db.Elecciones.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == comp.EleccionId, ct);

                var junta = await _db.Juntas.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == comp.JuntaId, ct);

                // 4. Variables de seguridad para evitar NullReferenceException en el HTML
                // Si el usuario es nulo, es un error de integridad grave
                if (user == null)
                    return Content("No se pudieron recuperar los datos del votante.", "text/plain");

                string nombreVotante = user.NombreCompleto ?? "Usuario no identificado";
                string cedulaVotante = user.Cedula ?? "N/D";
                string nombreProceso = proceso?.Nombre ?? "Proceso Electoral";
                string tituloEleccion = eleccion?.Titulo ?? "Elección General";
                string codigoJunta = junta?.Codigo ?? "N/A";
                string fotoUrl = user.FotoUrl ?? "";

                // 5. Generación del HTML con manejo de caracteres UTF-8
                var html = $@"
<!doctype html>
<html lang='es'>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width, initial-scale=1'/>
    <title>Comprobante de Votación</title>
    <style>
        body {{ font-family: sans-serif; background: #f8f9fa; padding: 20px; color: #333; }}
        .card {{ max-width: 600px; margin: auto; background: #fff; border-radius: 12px; padding: 25px; border: 1px solid #ddd; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ border-bottom: 2px solid #007bff; padding-bottom: 10px; margin-bottom: 20px; }}
        .info-grid {{ display: flex; gap: 20px; flex-wrap: wrap; }}
        .photo-box {{ border: 1px solid #eee; padding: 10px; background: #fafafa; border-radius: 8px; text-align: center; width: 150px; }}
        .details {{ flex: 1; }}
        .details p {{ margin: 8px 0; line-height: 1.4; }}
        .footer-note {{ font-size: 12px; color: #777; margin-top: 20px; border-top: 1px solid #eee; padding-top: 10px; }}
        .btn-print {{ margin-top: 20px; padding: 10px 20px; background: #007bff; color: #fff; border: none; border-radius: 6px; cursor: pointer; }}
        @media print {{ .btn-print {{ display: none; }} }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='header'>
            <h2 style='margin:0'>Certificado de Votación</h2>
        </div>

        <div class='info-grid'>
            <div class='photo-box'>
                <strong>Foto</strong><br/><br/>
                {(string.IsNullOrWhiteSpace(fotoUrl)
                            ? "<div style='color:#999; padding:40px 0;'>Sin foto</div>"
                            : $"<img src='{fotoUrl}' style='width:130px; height:130px; object-fit:cover; border-radius:6px;'/>")}
            </div>

            <div class='details'>
                <p><b>Votante:</b> {nombreVotante}</p>
                <p><b>Cédula:</b> {cedulaVotante}</p>
                <p><b>Proceso:</b> {nombreProceso}</p>
                <p><b>Elección:</b> {tituloEleccion}</p>
                <p><b>Junta:</b> {codigoJunta}</p>
                <p><b>Fecha de Emisión:</b> {comp.GeneradoUtc.ToLocalTime():yyyy-MM-dd HH:mm}</p>
            </div>
        </div>

        <div class='footer-note'>
            Este documento certifica que el ciudadano ha ejercido su derecho al voto de forma electrónica. 
            <strong>No revela el contenido de su elección.</strong>
        </div>

        <button class='btn-print' onclick='window.print()'>Imprimir Comprobante</button>
    </div>
</body>
</html>";

                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                // 6. Registro del error en los logs de Render para diagnóstico
                Console.WriteLine($"[CRITICAL ERROR - ComprobantePublico]: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return Content("Ocurrió un error inesperado al procesar su solicitud. Por favor, intente más tarde.", "text/plain");
            }
        }
        private static string CreatePublicToken()
        {
            // 32 bytes aleatorios, base64url (sin + / =)
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
        private async Task IncrementarResultadoAsync(Guid procesoId, Guid eleccionId, DimensionReporte dim, string dimValor, string opcion, CancellationToken ct)
        {
            dimValor = (dimValor ?? "").Trim();
            opcion = (opcion ?? "").Trim();

            var row = await _db.ResultadosAgregados.FirstOrDefaultAsync(x =>
                x.ProcesoElectoralId == procesoId &&
                x.EleccionId == eleccionId &&
                x.Dimension == dim &&
                x.DimensionValor == dimValor &&
                x.Opcion == opcion, ct);

            if (row == null)
            {
                row = new ResultadoAgregado
                {
                    Id = Guid.NewGuid(),
                    ProcesoElectoralId = procesoId,
                    EleccionId = eleccionId,
                    Dimension = dim,
                    DimensionValor = dimValor,
                    Opcion = opcion,
                    Votos = 1,
                    ActualizadoUtc = DateTime.UtcNow
                };
                _db.ResultadosAgregados.Add(row);
            }
            else
            {
                row.Votos += 1;
                row.ActualizadoUtc = DateTime.UtcNow;
            }
        }
    }
}
