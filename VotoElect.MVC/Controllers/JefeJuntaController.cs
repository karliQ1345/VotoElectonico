using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.Services;
using VotoElect.MVC.ViewModels;
namespace VotoElect.MVC.Controllers;

public class JefeJuntaController : Controller
{
    private readonly ApiService _api;

    public JefeJuntaController(ApiService api)
    {
        _api = api;
    }

    private string? Token() =>
        HttpContext.Session.GetString(SessionKeys.TokenJefe)
        ?? HttpContext.Session.GetString(SessionKeys.Token);
    private string? Cedula() => HttpContext.Session.GetString(SessionKeys.Cedula);

    [HttpGet]
    public async Task<IActionResult> Panel(CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var vm = new JefePanelVm();

        var proc = await _api.GetProcesoActivoAsync(token, ct);
        if (proc == null || !proc.Ok || proc.Data == null || string.IsNullOrWhiteSpace(proc.Data.ProcesoElectoralId))
        {
            vm.Error = proc?.Message ?? "No se pudo obtener el proceso activo.";
            return View(vm);
        }

        var procesoId = proc.Data.ProcesoElectoralId;
        vm.ProcesoElectoralId = procesoId;

        var p = await _api.GetJefePanelAsync(procesoId, token, ct);
        if (p == null || !p.Ok || p.Data == null)
            vm.Error = p?.Message ?? "No se pudo cargar el panel.";
        else
            vm.Panel = p.Data;

        if (TempData["ok"] is string ok) vm.Ok = ok;
        if (TempData["err"] is string err) vm.Error ??= err;

        if (TempData["verif_json"] is string json && !string.IsNullOrWhiteSpace(json))
        {
            vm.Verificacion = JsonSerializer.Deserialize<JefeVerificarVotanteResponseDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
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

        var proc = await _api.GetProcesoActivoAsync(token, ct);
        if (proc == null || !proc.Ok || proc.Data == null || string.IsNullOrWhiteSpace(proc.Data.ProcesoElectoralId))
        {
            TempData["err"] = proc?.Message ?? "No se pudo obtener el proceso activo.";
            return RedirectToAction(nameof(Panel));
        }

        var procesoId = proc.Data.ProcesoElectoralId;

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

        TempData["verif_json"] = JsonSerializer.Serialize(resp.Data);
        TempData["ok"] = resp.Data.Mensaje;

        return RedirectToAction(nameof(Panel));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Votar(CancellationToken ct)
    {
        var token = Token();
        var cedula = Cedula();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(cedula))
            return RedirectToAction("Index", "Acceso");

        var proc = await _api.GetProcesoActivoAsync(token, ct);
        if (proc == null || !proc.Ok || proc.Data == null || string.IsNullOrWhiteSpace(proc.Data.ProcesoElectoralId))
        {
            TempData["err"] = proc?.Message ?? "No se pudo obtener el proceso activo.";
            return RedirectToAction(nameof(Panel));
        }

        var procesoId = proc.Data.ProcesoElectoralId;

        var ver = await _api.JefeVerificarAsync(new JefeVerificarVotanteRequestDto
        {
            ProcesoElectoralId = procesoId,
            CedulaVotante = cedula
        }, token, ct);

        if (ver == null || !ver.Ok || ver.Data == null)
        {
            TempData["err"] = ver?.Message ?? "No se pudo habilitar el voto del jefe.";
            return RedirectToAction(nameof(Panel));
        }

        if (!ver.Data.Permitido)
        {
            // Ej: "Ya registró el voto" o "No está habilitado"
            TempData["err"] = ver.Data.Mensaje;
            TempData["verif_json"] = JsonSerializer.Serialize(ver.Data);
            return RedirectToAction(nameof(Panel));
        }

        if (string.IsNullOrWhiteSpace(ver.Data.CodigoUnico))
        {
            TempData["err"] = "No se generó código único para el jefe.";
            return RedirectToAction(nameof(Panel));
        }

        var codigo = ver.Data.CodigoUnico;

        var ini = await _api.IniciarVotacionAsync(new IniciarVotacionRequestDto
        {
            ProcesoElectoralId = procesoId,
            Cedula = cedula,
            CodigoUnico = codigo
        }, token, ct);

        if (ini == null || !ini.Ok || ini.Data == null || !ini.Data.Habilitado)
        {
            TempData["err"] = ini?.Message ?? ini?.Data?.Mensaje ?? "No se pudo iniciar votación.";
            return RedirectToAction(nameof(Panel));
        }

        HttpContext.Session.SetString(SessionKeys.CodigoUnico, codigo);

        return RedirectToAction("Papeleta", "Votantes");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarJunta(CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var proc = await _api.GetProcesoActivoAsync(token, ct);
        if (proc == null || !proc.Ok || proc.Data == null || string.IsNullOrWhiteSpace(proc.Data.ProcesoElectoralId))
        {
            TempData["err"] = proc?.Message ?? "No se pudo obtener el proceso activo.";
            return RedirectToAction(nameof(Panel));
        }

        var resp = await _api.FinalizarJuntaAsync(proc.Data.ProcesoElectoralId, token, ct);

        if (resp == null || !resp.Ok || resp.Data == null)
        {
            TempData["err"] = resp?.Message ?? "No se pudo finalizar la junta.";
            return RedirectToAction(nameof(Panel));
        }

        TempData["ok"] = resp.Data.Mensaje;
        return RedirectToAction(nameof(Panel));
    }
}