using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.Services;
using VotoElect.MVC.ViewModels;
using  VotoElect.MVC.Utils;
using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.Controllers;

public class AdminController : Controller
{
    private readonly ApiService _api;
    public AdminController(ApiService api) => _api = api;

    private string? Token() =>
         HttpContext.Session.GetString(SessionKeys.TokenAdmin)
         ?? HttpContext.Session.GetString(SessionKeys.Token);

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

        var inicioUtc = DateTime.SpecifyKind(form.InicioUtc, DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(form.FinUtc, DateTimeKind.Utc);

        if (finUtc <= inicioUtc)
        {
            TempData["err"] = "Fin debe ser mayor que Inicio.";
            return RedirectToAction(nameof(Procesos));
        }

        var resp = await _api.AdminCrearProcesoAsync(new()
        {
            Nombre = form.Nombre.Trim(),
            InicioUtc = inicioUtc,
            FinUtc = finUtc
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

    //--
    [HttpGet]
    public async Task<IActionResult> Candidatos(CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var vm = new AdminCandidatosVm();

        var procesos = await _api.AdminListarProcesosAsync(token, ct);
        vm.Procesos = procesos?.Data ?? new();

        if (TempData["ok"] is string ok) vm.Ok = ok;
        if (TempData["err"] is string err) vm.Error = err;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearEleccion(AdminCandidatosVm form, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var resp = await _api.AdminCrearEleccionAsync(new CrearEleccionRequestDto
        {
            ProcesoElectoralId = form.ProcesoElectoralId,
            Tipo = form.Tipo,
            Titulo = form.Titulo,
            MaxSeleccionIndividual = form.MaxSeleccionIndividual
        }, token, ct);

        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo crear la elección.";
        if (resp?.Data != null)
            TempData[resp?.Ok == true ? "ok" : "err"] += $" (Id: {resp.Data.Id})";

        return RedirectToAction(nameof(Candidatos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearLista(AdminCandidatosVm form, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var resp = await _api.AdminCrearListaAsync(new CrearListaRequestDto
        {
            EleccionId = form.EleccionId,
            Nombre = form.ListaNombre,
            Codigo = form.ListaCodigo,
            LogoUrl = form.ListaLogoUrl
        }, token, ct);

        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo crear la lista.";
        if (resp?.Data != null)
            TempData[resp?.Ok == true ? "ok" : "err"] += $" (Id: {resp.Data.Id})";

        return RedirectToAction(nameof(Candidatos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCandidato(AdminCandidatosVm form, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var resp = await _api.AdminCrearCandidatoAsync(new CrearCandidatoRequestDto
        {
            EleccionId = form.EleccionId,
            NombreCompleto = form.CandidatoNombre,
            Cargo = form.CandidatoCargo,
            FotoUrl = form.CandidatoFotoUrl,
            PartidoListaId = form.CandidatoPartidoListaId
        }, token, ct);

        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo crear el candidato.";
        if (resp?.Data != null)
            TempData[resp?.Ok == true ? "ok" : "err"] += $" (Id: {resp.Data.Id})";

        return RedirectToAction(nameof(Candidatos));
    }
    //--


    [HttpGet]
    public async Task<IActionResult> Padron(string? procesoId, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var vm = new AdminPadronVm();

        var procesos = await _api.AdminListarProcesosAsync(token, ct);
        vm.Procesos = procesos?.Data ?? new();

        if (!string.IsNullOrWhiteSpace(procesoId))
            vm.ProcesoElectoralId = procesoId;

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
