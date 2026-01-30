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
    [HttpGet]
    public async Task<IActionResult> Padron(CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Index", "Acceso");

        var vm = new AdminPadronVm();

        var procesos = await _api.AdminListarProcesosAsync(token, ct);
        vm.Procesos = procesos?.Data ?? new();

        if (TempData["ok"] is string ok) vm.Ok = ok;
        if (TempData["err"] is string err) vm.Error = err;
        if (TempData["padron_result"] is string json) vm.ResultJson = json;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CargarPadron(AdminPadronVm form, IFormFile excel, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Index", "Acceso");

        if (string.IsNullOrWhiteSpace(form.ProcesoElectoralId))
        {
            TempData["err"] = "Seleccione un proceso.";
            return RedirectToAction(nameof(Padron));
        }
        if (excel == null || excel.Length == 0)
        {
            TempData["err"] = "Seleccione un archivo Excel.";
            return RedirectToAction(nameof(Padron));
        }

        // 1) Excel -> List<PadronExcelRowDto>
        var rows = ExcelPadronParser.Leer(excel.OpenReadStream()); // helper (abajo)

        // 2) Enviar JSON a la API
        var resp = await _api.AdminCargarPadronAsync(form.ProcesoElectoralId, rows, token, ct);

        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo cargar padrón.";
        if (resp?.Data != null) TempData["padron_result"] = System.Text.Json.JsonSerializer.Serialize(resp.Data);

        return RedirectToAction(nameof(Padron));
    }

}


