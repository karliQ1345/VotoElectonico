using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.Services;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.ViewModels;

namespace VotoElect.MVC.Controllers;

public class JefeJuntaController : Controller
{
    private readonly ApiService _api;
    private readonly IConfiguration _cfg;

    public JefeJuntaController(ApiService api, IConfiguration cfg)
    {
        _api = api;
        _cfg = cfg;
    }

    [HttpGet]
    public async Task<IActionResult> Panel(CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var procesoId = _cfg["Votacion:ProcesoElectoralId"] ?? "";
        var vm = new JefeJuntaPanelVm { ProcesoElectoralId = procesoId };

        var resp = await _api.GetJefePanelAsync(procesoId, token, ct);
        vm.Panel = resp?.Data;
        vm.Error = resp == null ? "No se pudo conectar con la API." : (!resp.Ok ? resp.Message : null);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verificar(string cedulaVotante, CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var procesoId = _cfg["Votacion:ProcesoElectoralId"] ?? "";
        var vm = new JefeJuntaPanelVm { ProcesoElectoralId = procesoId, CedulaVotante = (cedulaVotante ?? "").Trim() };

        // Recargar panel
        var panelResp = await _api.GetJefePanelAsync(procesoId, token, ct);
        vm.Panel = panelResp?.Data;

        if (string.IsNullOrWhiteSpace(vm.CedulaVotante))
        {
            vm.Error = "Ingrese la cédula del votante.";
            return View("Panel", vm);
        }

        var req = new JefeVerificarVotanteRequestDto
        {
            ProcesoElectoralId = procesoId,
            CedulaVotante = vm.CedulaVotante
        };

        var resp = await _api.JefeVerificarAsync(req, token, ct);
        vm.Resultado = resp?.Data;
        vm.Error = resp == null ? "No se pudo conectar con la API." : (!resp.Ok ? resp.Message : null);

        return View("Panel", vm);
    }
}


