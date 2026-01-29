using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.Services;

namespace VotoElect.MVC.Controllers;

public class ResultadosController : Controller
{
    private readonly ApiService _api;
    private readonly IConfiguration _cfg;

    public ResultadosController(ApiService api, IConfiguration cfg)
    {
        _api = api;
        _cfg = cfg;
    }

    // GET /Resultados/Data?dimension=Provincia
    [HttpGet]
    public async Task<IActionResult> Data(string dimension, CancellationToken ct)
    {
        var procesoId = _cfg["Votacion:ProcesoElectoralId"] ?? "";
        var eleccionId = _cfg["Votacion:EleccionId"] ?? "";

        if (string.IsNullOrWhiteSpace(procesoId) || string.IsNullOrWhiteSpace(eleccionId))
            return BadRequest(new { ok = false, message = "Falta Votacion:ProcesoElectoralId o Votacion:EleccionId en appsettings.json" });

        dimension = (dimension ?? "").Trim();
        if (string.IsNullOrWhiteSpace(dimension)) dimension = "Provincia";

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
            dimension = resp.Data.Dimension,
            actualizadoUtc = resp.Data.ActualizadoUtc,
            items = resp.Data.Items
        });
    }
}

