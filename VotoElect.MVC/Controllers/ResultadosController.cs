using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.Services;

namespace VotoElect.MVC.Controllers;

[Route("[controller]")]
public class ResultadosController : Controller
{
    private readonly ApiService _api;

    public ResultadosController(ApiService api)
    {
        _api = api;
    }

    // GET /Resultados/Estado
    [HttpGet("Estado")]
    public async Task<IActionResult> Estado(CancellationToken ct)
    {
        // 1) Intenta proceso activo
        var act = await _api.GetProcesoActivoPublicAsync(ct);
        if (act?.Ok == true && act.Data != null && !string.IsNullOrWhiteSpace(act.Data.ProcesoElectoralId))
            return Json(new { ok = true, data = act.Data, source = "ACTIVO" });

        // 2) Si no hay activo, trae el último finalizado
        var fin = await _api.GetUltimoFinalizadoPublicAsync(ct);
        if (fin?.Ok == true && fin.Data != null && !string.IsNullOrWhiteSpace(fin.Data.ProcesoElectoralId))
            return Json(new { ok = true, data = fin.Data, source = "ULTIMO_FINALIZADO" });

        return Json(new { ok = false, message = fin?.Message ?? act?.Message ?? "Sin proceso." });
    }

    [HttpGet("BoletaMap")]
    public async Task<IActionResult> BoletaMap([FromQuery] string procesoId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(procesoId))
            return Json(new { ok = false, message = "procesoId requerido." });

        var boleta = await _api.GetBoletaActivaAsync(procesoId, ct);
        if (boleta?.Ok != true || boleta.Data == null)
            return Json(new { ok = false, message = boleta?.Message ?? "No se pudo obtener boleta." });

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // BLANCO siempre
        map["BLANCO"] = "Blanco";

        // Candidatos (GUID -> NombreCompleto)
        if (boleta.Data.Candidatos != null)
        {
            foreach (var c in boleta.Data.Candidatos)
            {
                if (!string.IsNullOrWhiteSpace(c.CandidatoId) && !string.IsNullOrWhiteSpace(c.NombreCompleto))
                    map[c.CandidatoId] = c.NombreCompleto.Trim();
            }
        }

        // Listas (GUID -> "Codigo - Nombre")
        if (boleta.Data.Listas != null)
        {
            foreach (var l in boleta.Data.Listas)
            {
                if (!string.IsNullOrWhiteSpace(l.PartidoListaId))
                {
                    var label = $"{(l.Codigo ?? "").Trim()} {(string.IsNullOrWhiteSpace(l.Nombre) ? "" : "- " + l.Nombre.Trim())}".Trim();
                    map[l.PartidoListaId] = string.IsNullOrWhiteSpace(label) ? l.PartidoListaId : label;
                }
            }
        }

        return Json(new
        {
            ok = true,
            procesoId = boleta.Data.ProcesoElectoralId,
            eleccionId = boleta.Data.EleccionId,
            tipoEleccion = boleta.Data.TipoEleccion,
            titulo = boleta.Data.Titulo,
            map
        });
    }

    [HttpGet("Nacional")]
    public async Task<IActionResult> Nacional([FromQuery] string procesoId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(procesoId))
            return Json(new { ok = false, message = "procesoId requerido." });

        // 1) Boleta para obtener EleccionId
        var boleta = await _api.GetBoletaActivaAsync(procesoId, ct);
        if (boleta?.Ok != true || boleta.Data == null || string.IsNullOrWhiteSpace(boleta.Data.EleccionId))
            return Json(new { ok = false, message = boleta?.Message ?? "No se pudo obtener boleta." });

        var eleccionId = boleta.Data.EleccionId;

        // 2) Pedir reporte nacional a la API
        var rep = await _api.GetReporteAsync(new ReporteFiltroDto
        {
            ProcesoElectoralId = procesoId,
            EleccionId = eleccionId,
            Dimension = "Nacional"
        }, ct);

        if (rep?.Ok != true || rep.Data == null)
            return Json(new { ok = false, message = rep?.Message ?? "Sin reporte." });

        var items = (rep.Data.Items ?? new List<ReporteItemDto>())
            .OrderByDescending(x => x.Votos)
            .ToList();

        var total = items.Sum(x => x.Votos);

        var blanco = items.FirstOrDefault(x =>
            (x.Opcion ?? "").Equals("BLANCO", StringComparison.OrdinalIgnoreCase));

        var top = items
            .Where(x => !((x.Opcion ?? "").Equals("BLANCO", StringComparison.OrdinalIgnoreCase)))
            .Take(2)
            .ToList();

        return Json(new
        {
            ok = true,
            actualizadoUtc = rep.Data.ActualizadoUtc,
            totalVotos = total,
            top1 = top.ElementAtOrDefault(0),
            top2 = top.ElementAtOrDefault(1),
            blanco,
            items
        });
    }

    [HttpGet("Tabla")]
    public async Task<IActionResult> Tabla([FromQuery] string dimension, [FromQuery] string procesoId, CancellationToken ct)
    {
        dimension = (dimension ?? "").Trim();
        if (string.IsNullOrWhiteSpace(dimension)) dimension = "Provincia";

        if (string.IsNullOrWhiteSpace(procesoId))
            return Json(new { ok = false, message = "procesoId requerido." });

        var boleta = await _api.GetBoletaActivaAsync(procesoId, ct);
        if (boleta?.Ok != true || boleta.Data == null || string.IsNullOrWhiteSpace(boleta.Data.EleccionId))
            return Json(new { ok = false, message = boleta?.Message ?? "No se pudo obtener boleta." });

        var eleccionId = boleta.Data.EleccionId;

        var rep = await _api.GetReporteAsync(new ReporteFiltroDto
        {
            ProcesoElectoralId = procesoId,
            EleccionId = eleccionId,
            Dimension = dimension
        }, ct);

        if (rep?.Ok != true || rep.Data == null)
            return Json(new { ok = false, message = rep?.Message ?? "Sin reporte." });

        var items = (rep.Data.Items ?? new List<ReporteItemDto>())
            .OrderByDescending(x => x.Votos)
            .Select(x => new
            {
                dimensionValor = x.DimensionValor,
                opcion = x.Opcion,
                votos = x.Votos
            })
            .ToList();

        return Json(new
        {
            ok = true,
            dimension = rep.Data.Dimension,
            actualizadoUtc = rep.Data.ActualizadoUtc,
            items
        });
    }

    [HttpGet]
    public async Task<IActionResult> JuntasReportadas(string procesoId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(procesoId))
            return Json(new { ok = false, message = "procesoId requerido." });

        var r = await _api.GetJuntasReportadasPublicAsync(procesoId, ct);
        if (r?.Ok != true) return Json(new { ok = false, message = r?.Message ?? "No se pudo obtener juntas." });

        return Json(new { ok = true, total = r.Data });
    }

    [HttpGet("Data")]
    public async Task<IActionResult> Data([FromQuery] string dimension, CancellationToken ct)
    {
        dimension = (dimension ?? "").Trim();
        if (string.IsNullOrWhiteSpace(dimension)) dimension = "Provincia";

        // Usa Estado: activo o último finalizado
        var act = await _api.GetProcesoActivoPublicAsync(ct);
        var p = (act?.Ok == true && act.Data != null) ? act : await _api.GetUltimoFinalizadoPublicAsync(ct);

        if (p == null || !p.Ok || p.Data == null || string.IsNullOrWhiteSpace(p.Data.ProcesoElectoralId))
            return Ok(new { ok = false, message = p?.Message ?? "No se pudo obtener proceso." });

        var procesoId = p.Data.ProcesoElectoralId;

        var boleta = await _api.GetBoletaActivaAsync(procesoId, ct);
        if (boleta == null || !boleta.Ok || boleta.Data == null || string.IsNullOrWhiteSpace(boleta.Data.EleccionId))
            return Ok(new { ok = false, message = boleta?.Message ?? "No hay elección activa para este proceso." });

        var eleccionId = boleta.Data.EleccionId;

        var resp = await _api.GetReporteAsync(new ReporteFiltroDto
        {
            ProcesoElectoralId = procesoId,
            EleccionId = eleccionId,
            Dimension = dimension
        }, ct);

        if (resp == null || !resp.Ok || resp.Data == null)
            return Ok(new { ok = false, message = resp?.Message ?? "No se pudo cargar reporte." });

        return Ok(new
        {
            ok = true,
            procesoId,
            estado = p.Data.Estado,
            dimension = resp.Data.Dimension,
            actualizadoUtc = resp.Data.ActualizadoUtc,
            items = resp.Data.Items
        });
    }

}