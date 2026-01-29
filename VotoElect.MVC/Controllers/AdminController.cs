using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.Services;
using VotoElect.MVC.ViewModels;

namespace VotoElect.MVC.Controllers;

public class AdminController : Controller
{
    private readonly ApiService _api;
    public AdminController(ApiService api) => _api = api;

    private string? Token() => HttpContext.Session.GetString(SessionKeys.Token);

    [HttpGet]
    public async Task<IActionResult> Procesos(CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var vm = new AdminProcesosVm();

        if (TempData["ok"] is string ok) vm.Ok = ok;
        if (TempData["err"] is string err) vm.Error = err;

        var resp = await _api.AdminListarProcesosAsync(token, ct);
        if (resp == null || !resp.Ok || resp.Data == null)
            vm.Error ??= resp?.Message ?? "No se pudo listar procesos.";
        else
            vm.Procesos = resp.Data;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProceso(AdminProcesosVm form, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        if (string.IsNullOrWhiteSpace(form.Nombre))
        {
            TempData["err"] = "Nombre requerido.";
            return RedirectToAction(nameof(Procesos));
        }

        var resp = await _api.AdminCrearProcesoAsync(new()
        {
            Nombre = form.Nombre.Trim(),
            InicioUtc = form.InicioUtc,
            FinUtc = form.FinUtc
        }, token, ct);

        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo crear proceso.";
        return RedirectToAction(nameof(Procesos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(string procesoId, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var resp = await _api.AdminActivarProcesoAsync(procesoId, token, ct);
        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo activar.";
        return RedirectToAction(nameof(Procesos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(string procesoId, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var resp = await _api.AdminFinalizarProcesoAsync(procesoId, token, ct);
        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo finalizar.";
        return RedirectToAction(nameof(Procesos));
    }

    [HttpGet]
    public IActionResult Dashboard()
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        return View();
    }

}


