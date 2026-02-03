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
        public async Task<ActionResult<ApiResponse<CargaPadronResponseDto>>> Cargar(
            Guid procesoId,
            [FromBody] List<PadronExcelRowDto> rows,
            CancellationToken ct)
        {
            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<CargaPadronResponseDto>.Fail("Proceso no existe."));

            if (rows == null || rows.Count == 0)
                return BadRequest(ApiResponse<CargaPadronResponseDto>.Fail("No hay filas para procesar."));

            int insertados = 0, actualizados = 0, conError = 0;
            var errores = new List<string>();

            // Normaliza
            foreach (var r in rows)
            {
                r.Cedula = r.Cedula?.Trim() ?? "";
                r.NombreCompleto = r.NombreCompleto?.Trim() ?? "";
                r.Email = r.Email?.Trim() ?? "";
                r.JuntaCodigo = r.JuntaCodigo?.Trim() ?? "";
            }

            // ===== PASADA 1: JEFES =====
            var jefes = rows.Where(x => x.Rol == RolTipo.JefeJunta).ToList();

            foreach (var r in jefes)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(r.Cedula) || string.IsNullOrWhiteSpace(r.NombreCompleto) || string.IsNullOrWhiteSpace(r.Email))
                        throw new Exception("Faltan campos (Cedula/Nombre/Email) para JEFE.");
                    if (string.IsNullOrWhiteSpace(r.JuntaCodigo))
                        throw new Exception("JuntaCodigo requerido para JEFE.");

                    // 1) Usuario jefe (crear/actualizar)
                    var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == r.Cedula, ct);
                    if (user == null)
                    {
                        user = new Usuario
                        {
                            Id = Guid.NewGuid(),
                            Cedula = r.Cedula,
                            NombreCompleto = r.NombreCompleto,
                            Email = r.Email,
                            Provincia = r.Provincia,
                            Canton = r.Canton,
                            Parroquia = r.Parroquia,
                            Genero = r.Genero,
                            FotoUrl = r.FotoUrl,
                            Activo = true,
                            CreadoUtc = DateTime.UtcNow
                        };
                        _db.Usuarios.Add(user);
                        insertados++;
                    }
                    else
                    {
                        user.NombreCompleto = r.NombreCompleto;
                        user.Email = r.Email;
                        user.Provincia = r.Provincia;
                        user.Canton = r.Canton;
                        user.Parroquia = r.Parroquia;
                        user.Genero = r.Genero;
                        user.FotoUrl = r.FotoUrl;

                        // 🔥 clave: si existía inactivo, lo reactivas
                        user.Activo = true;

                        actualizados++;
                    }

                    // 2) Rol JefeJunta (evitar duplicado)
                    var yaTieneRolJefe = await _db.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == user.Id && ur.Rol == RolTipo.JefeJunta, ct);
                    if (!yaTieneRolJefe)
                        _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, Rol = RolTipo.JefeJunta });

                    // 2.1) El jefe también es votante (evitar duplicado)
                    var yaTieneRolVot = await _db.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == user.Id && ur.Rol == RolTipo.Votante, ct);
                    if (!yaTieneRolVot)
                        _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, Rol = RolTipo.Votante });

                    // 3) Junta (crear/actualizar) y asignar jefe
                    var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.Codigo == r.JuntaCodigo, ct);
                    if (junta == null)
                    {
                        junta = new Junta
                        {
                            Id = Guid.NewGuid(),
                            Codigo = r.JuntaCodigo,
                            Provincia = r.Provincia ?? "",
                            Canton = r.Canton ?? "",
                            Parroquia = r.Parroquia ?? "",
                            Recinto = null,
                            Activa = true,
                            JefeJuntaUsuarioId = user.Id
                        };
                        _db.Juntas.Add(junta);
                    }
                    else
                    {
                        junta.Provincia = r.Provincia ?? junta.Provincia;
                        junta.Canton = r.Canton ?? junta.Canton;
                        junta.Parroquia = r.Parroquia ?? junta.Parroquia;
                        junta.JefeJuntaUsuarioId = user.Id;
                        junta.Activa = true;
                    }

                    // 4) 🔥 IMPORTANTE: el JEFE también debe existir en el PADRÓN del proceso
                    var prJefe = await _db.PadronRegistros.FirstOrDefaultAsync(x =>
                        x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

                    if (prJefe == null)
                    {
                        prJefe = new PadronRegistro
                        {
                            Id = Guid.NewGuid(),
                            ProcesoElectoralId = procesoId,
                            UsuarioId = user.Id,
                            JuntaId = junta.Id,
                            YaVoto = false
                        };
                        _db.PadronRegistros.Add(prJefe);
                    }
                    else
                    {
                        prJefe.JuntaId = junta.Id;
                    }

                    // 5) (Opcional) Código para el jefe también (consistencia)
                    var cvJefe = await _db.CodigosVotacion.FirstOrDefaultAsync(x =>
                        x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);

                    if (cvJefe == null)
                    {
                        var code = SecurityHelpers.GenerateNumericCode(6);
                        cvJefe = new CodigoVotacion
                        {
                            Id = Guid.NewGuid(),
                            ProcesoElectoralId = procesoId,
                            UsuarioId = user.Id,
                            CodigoHash = SecurityHelpers.HashWithSalt(code),
                            Usado = false,
                            CreadoUtc = DateTime.UtcNow
                        };
                        _db.CodigosVotacion.Add(cvJefe);
                    }
                }
                catch (Exception ex)
                {
                    conError++;
                    errores.Add($"JEFE Cedula {r?.Cedula}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync(ct);

            // ===== PASADA 2: VOTANTES =====
            var votantes = rows.Where(x => x.Rol == RolTipo.Votante).ToList();

            foreach (var r in votantes)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(r.Cedula) || string.IsNullOrWhiteSpace(r.NombreCompleto) || string.IsNullOrWhiteSpace(r.Email))
                        throw new Exception("Faltan campos (Cedula/Nombre/Email) para VOTANTE.");
                    if (string.IsNullOrWhiteSpace(r.JuntaCodigo))
                        throw new Exception("JuntaCodigo requerido para VOTANTE.");

                    var junta = await _db.Juntas.FirstOrDefaultAsync(x => x.Codigo == r.JuntaCodigo, ct);
                    if (junta == null)
                        throw new Exception($"La junta '{r.JuntaCodigo}' no existe. Debe venir una fila Rol=JefeJunta para esa junta.");

                    if (junta.JefeJuntaUsuarioId == Guid.Empty)
                        throw new Exception($"La junta '{r.JuntaCodigo}' no tiene jefe asignado.");

                    var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Cedula == r.Cedula, ct);
                    if (user == null)
                    {
                        user = new Usuario
                        {
                            Id = Guid.NewGuid(),
                            Cedula = r.Cedula,
                            NombreCompleto = r.NombreCompleto,
                            Email = r.Email,
                            Provincia = r.Provincia,
                            Canton = r.Canton,
                            Parroquia = r.Parroquia,
                            Genero = r.Genero,
                            FotoUrl = r.FotoUrl,
                            Activo = true,
                            CreadoUtc = DateTime.UtcNow
                        };
                        _db.Usuarios.Add(user);
                        insertados++;
                    }
                    else
                    {
                        user.NombreCompleto = r.NombreCompleto;
                        user.Email = r.Email;
                        user.Provincia = r.Provincia;
                        user.Canton = r.Canton;
                        user.Parroquia = r.Parroquia;
                        user.Genero = r.Genero;
                        user.FotoUrl = r.FotoUrl;

                        // 🔥 clave: reactivar si estaba inactivo
                        user.Activo = true;

                        actualizados++;
                    }

                    var yaTieneRolVot = await _db.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == user.Id && ur.Rol == RolTipo.Votante, ct);
                    if (!yaTieneRolVot)
                        _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, Rol = RolTipo.Votante });

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

                    var cv = await _db.CodigosVotacion.FirstOrDefaultAsync(x => x.ProcesoElectoralId == procesoId && x.UsuarioId == user.Id, ct);
                    if (cv == null)
                    {
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
                    errores.Add($"VOTANTE Cedula {r?.Cedula}: {ex.Message}");
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
