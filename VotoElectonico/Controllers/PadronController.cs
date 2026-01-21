using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Padron;
using VotoElectonico.Models;
using VotoElectonico.Models.Enums;
using VotoElectonico.Utils;

namespace VotoElectonico.Controllers
{
    [Authorize(Roles = nameof(RolTipo.Administrador))]
    [ApiController]
    [Route("api/padron")]
    public class PadronController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        public PadronController(ApplicationDbContext db) => _db = db;

        // POST api/padron/{procesoId}/carga
        [HttpPost("{procesoId:guid}/carga")]
        public async Task<ActionResult<ApiResponse<CargaPadronResponseDto>>> Cargar(Guid procesoId, [FromBody] List<PadronExcelRowDto> rows, CancellationToken ct)
        {
            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<CargaPadronResponseDto>.Fail("Proceso no existe."));

            if (rows == null || rows.Count == 0)
                return BadRequest(ApiResponse<CargaPadronResponseDto>.Fail("No hay filas para procesar."));

            int insertados = 0, actualizados = 0, conError = 0;
            var errores = new List<string>();

            foreach (var r in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(r.Cedula) || string.IsNullOrWhiteSpace(r.NombreCompleto) || string.IsNullOrWhiteSpace(r.Email))
                        throw new Exception("Faltan campos (Cedula/Nombre/Email).");

                    if (string.IsNullOrWhiteSpace(r.JuntaCodigo))
                        throw new Exception("JuntaCodigo requerido.");

                    // 1) Junta (crear si no existe)
                    var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.Codigo == r.JuntaCodigo.Trim(), ct);
                    if (junta == null)
                    {
                        junta = new Junta
                        {
                            Id = Guid.NewGuid(),
                            Codigo = r.JuntaCodigo.Trim(),
                            Provincia = r.Provincia ?? "",
                            Canton = r.Canton ?? "",
                            Parroquia = r.Parroquia ?? "",
                            Recinto = null,
                            Activa = true,

                            JefeJuntaUsuarioId = Guid.Empty
                        };
                        _db.Juntas.Add(junta);
                    }

                    // 2) Usuario (crear/actualizar)
                    var ced = r.Cedula.Trim();
                    var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == ced, ct);
                    if (user == null)
                    {
                        user = new Usuario
                        {
                            Id = Guid.NewGuid(),
                            Cedula = ced,
                            NombreCompleto = r.NombreCompleto.Trim(),
                            Email = r.Email.Trim(),
                            Provincia = r.Provincia,
                            Canton = r.Canton,
                            Parroquia = r.Parroquia,
                            Genero = r.Genero,
                            FotoUrl = r.FotoUrl,
                            Activo = true,
                            CreadoUtc = DateTime.UtcNow
                        };
                        _db.Usuarios.Add(user);

                        // rol votante por defecto
                        _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, Rol = RolTipo.Votante });
                        insertados++;
                    }
                    else
                    {
                        user.NombreCompleto = r.NombreCompleto.Trim();
                        user.Email = r.Email.Trim();
                        user.Provincia = r.Provincia;
                        user.Canton = r.Canton;
                        user.Parroquia = r.Parroquia;
                        user.Genero = r.Genero;
                        user.FotoUrl = r.FotoUrl;
                        actualizados++;
                    }

                    // 3) PadronRegistro (por proceso + usuario)
                    var pr = await _db.PadronRegistros.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
                    if (pr == null)
                    {
                        pr = new PadronRegistro
                        {
                            Id = Guid.NewGuid(),
                            ProcesoElectoralId = procesoId,
                            UsuarioId = user.Id,
                            JuntaId = junta.Id,
                            YaVoto = false
                        };
                        _db.PadronRegistros.Add(pr);
                    }
                    else
                    {
                        pr.JuntaId = junta.Id;
                    }

                    // 4) CodigoVotacion (por proceso + usuario)
                    var cv = await _db.CodigosVotacion.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
                    if (cv == null)
                    {
                        // Generamos un código inicial (no se guarda plano)
                        var code = SecurityHelpers.GenerateNumericCode(6);
                        cv = new CodigoVotacion
                        {
                            Id = Guid.NewGuid(),
                            ProcesoElectoralId = procesoId,
                            UsuarioId = user.Id,
                            CodigoHash = SecurityHelpers.HashWithSalt(code),
                            Usado = false,
                            CreadoUtc = DateTime.UtcNow
                        };
                        _db.CodigosVotacion.Add(cv);
                    }
                }
                catch (Exception ex)
                {
                    conError++;
                    errores.Add($"Cedula {r?.Cedula}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync(ct);

            var resp = new CargaPadronResponseDto
            {
                Total = rows.Count,
                Insertados = insertados,
                Actualizados = actualizados,
                ConError = conError,
                Errores = errores
            };

            return Ok(ApiResponse<CargaPadronResponseDto>.Success(resp, "Carga de padrón procesada."));
        }
    }
}
