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
    public async Task<IActionResult> CrearProceso(AdminProcesosVm form, bool continuarCandidatos, CancellationToken ct)
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

            FinUtc = finUtc,
            Tipo = form.TipoProceso

        }, token, ct);

        TempData[resp?.Ok == true ? "ok" : "err"] = resp?.Message ?? "No se pudo crear proceso.";

        if (resp?.Ok == true && continuarCandidatos && resp.Data != null)
        {
            return RedirectToAction(nameof(Candidatos), new
            {
                procesoId = resp.Data.Id,
                tipo = form.TipoProceso,
                titulo = form.Nombre
            });
        }

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
    public async Task<IActionResult> Candidatos(string? procesoId, string? tipo, string? titulo, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        var vm = new AdminCandidatosVm();

        var procesos = await _api.AdminListarProcesosAsync(token, ct);
        vm.Procesos = procesos?.Data ?? new();

        if (!string.IsNullOrWhiteSpace(procesoId))
            vm.ProcesoElectoralId = procesoId;
        if (!string.IsNullOrWhiteSpace(tipo))
            vm.Tipo = tipo;
        if (!string.IsNullOrWhiteSpace(titulo))
        {
            vm.Titulo = titulo;
        }
        else if (!string.IsNullOrWhiteSpace(vm.ProcesoElectoralId))
        {
            var proceso = vm.Procesos.FirstOrDefault(p => p.ProcesoElectoralId == vm.ProcesoElectoralId);
            if (proceso != null)
                vm.Titulo = proceso.Nombre;
        }

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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CargarCandidatos(AdminCandidatosVm form, IFormFile excel, CancellationToken ct)
    {
        var token = Token();
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Acceso");

        if (string.IsNullOrWhiteSpace(form.CargaEleccionId))
        {

            if (string.IsNullOrWhiteSpace(form.ProcesoElectoralId))
            {
                TempData["err"] = "Seleccione un proceso para crear la elección.";
                return RedirectToAction(nameof(Candidatos));
            }

            var procesos = await _api.AdminListarProcesosAsync(token, ct);
            var proceso = procesos?.Data?.FirstOrDefault(p => p.ProcesoElectoralId == form.ProcesoElectoralId);

            var titulo = !string.IsNullOrWhiteSpace(proceso?.Nombre)
                ? proceso.Nombre
                : string.IsNullOrWhiteSpace(form.Titulo)
                    ? $"Elección {DateTime.UtcNow:yyyy-MM-dd}"
                    : form.Titulo.Trim();

            var crearEleccion = await _api.AdminCrearEleccionAsync(new CrearEleccionRequestDto
            {
                ProcesoElectoralId = form.ProcesoElectoralId,
                Tipo = form.Tipo,
                Titulo = titulo,
                MaxSeleccionIndividual = form.MaxSeleccionIndividual
            }, token, ct);

            if (crearEleccion?.Ok != true || crearEleccion.Data == null)
            {
                TempData["err"] = crearEleccion?.Message ?? "No se pudo crear la elección para la carga masiva.";
                return RedirectToAction(nameof(Candidatos));
            }

            form.CargaEleccionId = crearEleccion.Data.Id;
        }

        if (excel == null || excel.Length == 0)
        {
            TempData["err"] = "Seleccione un archivo Excel.";
            return RedirectToAction(nameof(Candidatos));
        }

        var rows = ExcelCandidatosParser.Leer(excel.OpenReadStream());
        if (rows.Count == 0)
        {
            TempData["err"] = "No se encontraron filas válidas en el Excel.";
            return RedirectToAction(nameof(Candidatos));
        }

        var okCount = 0;
        var errorNames = new List<string>();

        foreach (var row in rows)
        {
            row.EleccionId = form.CargaEleccionId;
            var resp = await _api.AdminCrearCandidatoAsync(row, token, ct);
            if (resp?.Ok == true)
            {
                okCount++;
            }
            else
            {
                errorNames.Add(row.NombreCompleto);
            }
        }

        var message = $"Candidatos cargados: {okCount}/{rows.Count}.";
        if (errorNames.Count > 0)
            message += $" Fallidos: {string.Join(", ", errorNames)}.";

        TempData[okCount == rows.Count ? "ok" : "err"] = message;
        return RedirectToAction(nameof(Candidatos));
    }
    //-----



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
