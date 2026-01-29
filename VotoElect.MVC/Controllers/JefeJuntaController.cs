using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.Services;
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

    private string? Token() => HttpContext.Session.GetString(SessionKeys.Token);
    private string? Cedula() => HttpContext.Session.GetString(SessionKeys.Cedula);

    [HttpGet]
    public async Task<IActionResult> Panel(CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var procesoId = _cfg["Votacion:ProcesoElectoralId"] ?? "";
        var vm = new JefePanelVm { ProcesoElectoralId = procesoId };

        if (string.IsNullOrWhiteSpace(procesoId))
        {
            vm.Error = "Configura Votacion:ProcesoElectoralId en appsettings.json";
            return View(vm);
        }

        var p = await _api.GetJefePanelAsync(procesoId, token, ct);
        if (p == null || !p.Ok || p.Data == null)
            vm.Error = p?.Message ?? "No se pudo cargar el panel.";
        else
            vm.Panel = p.Data;

        if (TempData["ok"] is string ok) vm.Ok = ok;
        if (TempData["err"] is string err) vm.Error ??= err;

        if (TempData["verif_json"] is string json && !string.IsNullOrWhiteSpace(json))
        {
            vm.Verificacion = JsonSerializer.Deserialize<JefeVerificarVotanteResponseDto>(json);
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verificar(string cedulaVotante, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var procesoId = _cfg["Votacion:ProcesoElectoralId"] ?? "";
        if (string.IsNullOrWhiteSpace(procesoId))
        {
            TempData["err"] = "Configura Votacion:ProcesoElectoralId en appsettings.json";
            return RedirectToAction(nameof(Panel));
        }

        var ced = (cedulaVotante ?? "").Trim();
        if (string.IsNullOrWhiteSpace(ced))
        {
            TempData["err"] = "Ingrese la cédula del votante.";
            return RedirectToAction(nameof(Panel));
        }

        var resp = await _api.JefeVerificarAsync(new JefeVerificarVotanteRequestDto
        {
            ProcesoElectoralId = procesoId,
            CedulaVotante = ced
        }, token, ct);

        if (resp == null || !resp.Ok || resp.Data == null)
        {
            TempData["err"] = resp?.Message ?? "No se pudo verificar.";
            return RedirectToAction(nameof(Panel));
        }

        // Guardar resultado para mostrarlo en Panel
        TempData["verif_json"] = JsonSerializer.Serialize(resp.Data);
        TempData["ok"] = resp.Data.Mensaje;

        return RedirectToAction(nameof(Panel));
    }

    // Jefe vota sin pedir código en pantalla
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Votar(CancellationToken ct)
    {
        var token = Token();
        var cedula = Cedula();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(cedula))
            return RedirectToAction("Index", "Acceso");

        var procesoId = _cfg["Votacion:ProcesoElectoralId"] ?? "";
        if (string.IsNullOrWhiteSpace(procesoId))
        {
            TempData["err"] = "Configura Votacion:ProcesoElectoralId en appsettings.json";
            return RedirectToAction(nameof(Panel));
        }

        // 1) Generar código (sin mostrarlo al jefe como entrada)
        var ver = await _api.JefeVerificarAsync(new JefeVerificarVotanteRequestDto
        {
            ProcesoElectoralId = procesoId,
            CedulaVotante = cedula
        }, token, ct);

        if (ver == null || !ver.Ok || ver.Data == null || !ver.Data.Permitido || string.IsNullOrWhiteSpace(ver.Data.CodigoUnico))
        {
            TempData["err"] = ver?.Message ?? ver?.Data?.Mensaje ?? "No se pudo habilitar el voto del jefe.";
            return RedirectToAction(nameof(Panel));
        }

        var codigo = ver.Data.CodigoUnico;

        // 2) Iniciar votación
        var ini = await _api.IniciarVotacionAsync(new IniciarVotacionRequestDto
        {
            ProcesoElectoralId = procesoId,
            Cedula = cedula,
            CodigoUnico = codigo
        }, ct);

        if (ini == null || !ini.Ok || ini.Data == null || !ini.Data.Habilitado)
        {
            TempData["err"] = ini?.Message ?? ini?.Data?.Mensaje ?? "No se pudo iniciar votación.";
            return RedirectToAction(nameof(Panel));
        }

        // 3) Guardar código en sesión para emitir voto
        HttpContext.Session.SetString(SessionKeys.CodigoUnico, codigo);

        return RedirectToAction("Papeleta", "Votantes");
    }
}




