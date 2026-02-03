using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.DTOs.Common;
using VotoElectonico.DTOs.Elecciones;
using VotoElectonico.Models;
using VotoElectonico.Models.Enums;

namespace VotoElectonico.Controllers
{
    [Authorize(Roles = nameof(RolTipo.Administrador))]
    [ApiController]
    [Route("api/elecciones")]
    public class EleccionesController : BaseApiController
    {
        private readonly ApplicationDbContext _db;
        public EleccionesController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<IdResponseDto>>> CrearEleccion([FromBody] CrearEleccionRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.ProcesoElectoralId, out var procesoId))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("ProcesoElectoralId inválido."));

            var proceso = await _db.ProcesosElectorales.FirstOrDefaultAsync(x => x.Id == procesoId, ct);
            if (proceso == null) return NotFound(ApiResponse<IdResponseDto>.Fail("Proceso no existe."));

            if (!Enum.TryParse<EleccionTipo>(req.Tipo, out var tipo))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("Tipo de elección inválido."));

            if (string.IsNullOrWhiteSpace(req.Titulo))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("Titulo requerido."));

            if (tipo == EleccionTipo.Asambleistas && (!req.MaxSeleccionIndividual.HasValue || req.MaxSeleccionIndividual.Value <= 0))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("MaxSeleccionIndividual requerido para Asambleistas."));

            var e = new Eleccion
            {
                Id = Guid.NewGuid(),
                ProcesoElectoralId = procesoId,
                Tipo = tipo,
                Titulo = req.Titulo.Trim(),
                MaxSeleccionIndividual = tipo == EleccionTipo.Asambleistas ? req.MaxSeleccionIndividual : null,
                Activa = true
            };

            _db.Elecciones.Add(e);
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = e.Id.ToString() }, "Elección creada."));
        }

        [HttpPost("listas")]
        public async Task<ActionResult<ApiResponse<IdResponseDto>>> CrearLista([FromBody] CrearListaRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.EleccionId, out var eleccionId))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("EleccionId inválido."));

            var eleccion = await _db.Elecciones.FirstOrDefaultAsync(x => x.Id == eleccionId, ct);
            if (eleccion == null) return NotFound(ApiResponse<IdResponseDto>.Fail("Elección no existe."));

            if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Codigo))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("Nombre y Código son requeridos."));

            var existeCodigo = await _db.PartidosListas.AnyAsync(x => x.EleccionId == eleccionId && x.Codigo == req.Codigo.Trim(), ct);
            if (existeCodigo) return BadRequest(ApiResponse<IdResponseDto>.Fail("Código de lista ya existe en esa elección."));

            var lista = new PartidoLista
            {
                Id = Guid.NewGuid(),
                EleccionId = eleccionId,
                Nombre = req.Nombre.Trim(),
                Codigo = req.Codigo.Trim(),
                LogoUrl = req.LogoUrl
            };

            _db.PartidosListas.Add(lista);
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = lista.Id.ToString() }, "Lista creada."));
        }

        [HttpPost("candidatos")]
        public async Task<ActionResult<ApiResponse<IdResponseDto>>> CrearCandidato([FromBody] CrearCandidatoRequestDto req, CancellationToken ct)
        {
            if (!Guid.TryParse(req.EleccionId, out var eleccionId))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("EleccionId inválido."));

            var eleccion = await _db.Elecciones.FirstOrDefaultAsync(x => x.Id == eleccionId, ct);
            if (eleccion == null) return NotFound(ApiResponse<IdResponseDto>.Fail("Elección no existe."));

            if (string.IsNullOrWhiteSpace(req.NombreCompleto) || string.IsNullOrWhiteSpace(req.FotoUrl))
                return BadRequest(ApiResponse<IdResponseDto>.Fail("NombreCompleto y FotoUrl son requeridos."));

            Guid? listaId = null;
            var partidoListaRaw = req.PartidoListaId?.Trim();
            if (!string.IsNullOrWhiteSpace(partidoListaRaw))
            {
                if (Guid.TryParse(partidoListaRaw, out var lid))
                {
                    var listaExiste = await _db.PartidosListas.AnyAsync(x => x.Id == lid && x.EleccionId == eleccionId, ct);
                    if (!listaExiste) return BadRequest(ApiResponse<IdResponseDto>.Fail("Lista no existe para esa elección."));
                    listaId = lid;
                }
                else
                {
                    var codigo = partidoListaRaw.ToUpperInvariant();
                    var lista = await _db.PartidosListas
                        .FirstOrDefaultAsync(x => x.EleccionId == eleccionId && x.Codigo.ToUpper() == codigo, ct);
                    if (lista == null)
                    {
                        lista = new PartidoLista
                        {
                            Id = Guid.NewGuid(),
                            EleccionId = eleccionId,
                            Nombre = codigo,
                            Codigo = codigo,
                            LogoUrl = null
                        };
                        _db.PartidosListas.Add(lista);
                        await _db.SaveChangesAsync(ct);
                    }
                    listaId = lista.Id;
                }
            }
            var c = new Candidato
            {
                Id = Guid.NewGuid(),
                EleccionId = eleccionId,
                PartidoListaId = listaId,
                NombreCompleto = req.NombreCompleto.Trim(),
                Cargo = req.Cargo,
                FotoUrl = req.FotoUrl.Trim(),
                Activo = true
            };

            _db.Candidatos.Add(c);
            await _db.SaveChangesAsync(ct);

            return Ok(ApiResponse<IdResponseDto>.Success(new IdResponseDto { Id = c.Id.ToString() }, "Candidato creado."));
        }
    }
}
