using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.Services;
using VotoElect.MVC.ViewModels;

namespace VotoElect.MVC.Controllers;

public class VotantesController : Controller
{
    private readonly ApiService _api;
    private readonly IConfiguration _cfg;

    public VotantesController(ApiService api, IConfiguration cfg)
    {
        _api = api;
        _cfg = cfg;
    }

    private string? CedulaSesion() => HttpContext.Session.GetString(SessionKeys.Cedula);
    private string? CodigoSesion() => HttpContext.Session.GetString(SessionKeys.CodigoUnico);

    [HttpGet]
    public IActionResult Codigo()
    {
        if (CedulaSesion() == null) return RedirectToAction("Index", "Acceso");
        return View(new VotantesCodigoVm());
    }

    [HttpPost]
    public async Task<IActionResult> Codigo(string codigoPad, CancellationToken ct)
    {
        var cedula = CedulaSesion();
        if (cedula == null) return RedirectToAction("Index", "Acceso");

        var procesoId = _cfg["Votacion:ProcesoElectoralId"];
        if (string.IsNullOrWhiteSpace(procesoId))
            return BadRequest("Configura Votacion:ProcesoElectoralId en appsettings.json");

        var vm = new VotantesCodigoVm { CodigoUnico = codigoPad?.Trim() ?? "" };
        if (string.IsNullOrWhiteSpace(vm.CodigoUnico))
        {
            vm.Error = "Código requerido.";
            return View(vm);
        }

        var resp = await _api.IniciarVotacionAsync(new IniciarVotacionRequestDto
        {
            ProcesoElectoralId = procesoId!,
            Cedula = cedula,
            CodigoUnico = vm.CodigoUnico
        }, ct);

        if (resp == null || !resp.Ok || resp.Data == null)
        {
            vm.Error = resp?.Message ?? "No se pudo iniciar votación.";
            return View(vm);
        }

        if (!resp.Data.Habilitado)
        {
            vm.Error = resp.Data.Mensaje;
            return View(vm);
        }

        // Guardar el código único para emitir voto
        HttpContext.Session.SetString(SessionKeys.CodigoUnico, vm.CodigoUnico);

        return RedirectToAction(nameof(Papeleta));
    }

    [HttpGet]
    public async Task<IActionResult> Papeleta(CancellationToken ct)
    {
        if (CedulaSesion() == null) return RedirectToAction("Index", "Acceso");
        if (CodigoSesion() == null) return RedirectToAction(nameof(Codigo));

        var procesoId = _cfg["Votacion:ProcesoElectoralId"];
        var eleccionId = _cfg["Votacion:EleccionId"];
        if (string.IsNullOrWhiteSpace(procesoId) || string.IsNullOrWhiteSpace(eleccionId))
            return BadRequest("Configura Votacion:ProcesoElectoralId y Votacion:EleccionId en appsettings.json");

        var resp = await _api.GetBoletaAsync(procesoId!, eleccionId!, ct);
        if (resp == null || !resp.Ok || resp.Data == null)
        {
            return View(new VotantesPapeletaVm { Error = resp?.Message ?? "No se pudo cargar la boleta." });
        }

        return View(new VotantesPapeletaVm { Boleta = resp.Data });
    }

    [HttpPost]
    public IActionResult Confirmar(
        string procesoElectoralId,
        string eleccionId,
        string tipoEleccion,
        string? opcionPresidente,
        string? partidoListaId,
        List<string>? candidatoIds)
    {
        var vm = new VotantesConfirmarVm
        {
            ProcesoElectoralId = procesoElectoralId,
            EleccionId = eleccionId,
            TipoEleccion = tipoEleccion,
            OpcionPresidente = opcionPresidente?.Trim(),
            PartidoListaId = string.IsNullOrWhiteSpace(partidoListaId) ? null : partidoListaId.Trim(),
            CandidatoIds = candidatoIds ?? new List<string>()
        };

        // resumen
        if (tipoEleccion == "Presidente_SiNoBlanco")
        {
            if (string.IsNullOrWhiteSpace(vm.OpcionPresidente))
            {
                vm.Error = "Seleccione una opción (SI/NO/BLANCO).";
                return View(vm);
            }
            vm.Resumen = $"Presidente: {vm.OpcionPresidente}";
        }
        else
        {
            var votoPlancha = !string.IsNullOrWhiteSpace(vm.PartidoListaId);
            var votoIndividual = vm.CandidatoIds.Count > 0;

            if (votoPlancha == votoIndividual)
            {
                vm.Error = "Debes elegir plancha o candidatos individuales (solo uno).";
                return View(vm);
            }

            vm.Resumen = votoPlancha
                ? $"Asambleístas (Plancha): {vm.PartidoListaId}"
                : $"Asambleístas (Individual): {vm.CandidatoIds.Count} candidato(s)";
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Emitir(VotantesConfirmarVm vm, CancellationToken ct)
    {
        var cedula = CedulaSesion();
        var codigo = CodigoSesion();
        if (cedula == null) return RedirectToAction("Index", "Acceso");
        if (codigo == null) return RedirectToAction(nameof(Codigo));

        var req = new EmitirVotoRequestDto
        {
            ProcesoElectoralId = vm.ProcesoElectoralId,
            EleccionId = vm.EleccionId,
            Cedula = cedula,
            CodigoUnico = codigo
        };

        if (vm.TipoEleccion == "Presidente_SiNoBlanco")
        {
            req.OpcionPresidente = vm.OpcionPresidente;
        }
        else
        {
            req.PartidoListaId = vm.PartidoListaId;
            req.CandidatoIds = vm.CandidatoIds.Count > 0 ? vm.CandidatoIds : null;
        }

        var resp = await _api.EmitirVotoAsync(req, ct);
        if (resp == null || !resp.Ok || resp.Data == null)
        {
            vm.Error = resp?.Message ?? "No se pudo emitir el voto.";
            return View("Confirmar", vm);
        }

        // Limpia el código para que no re-emita accidentalmente
        HttpContext.Session.Remove(SessionKeys.CodigoUnico);

        return RedirectToAction(nameof(Comprobante), new
        {
            enviado = resp.Data.PapeletaEnviada,
            email = resp.Data.EmailEnmascarado,
            msg = resp.Data.Mensaje
        });
    }

    [HttpGet]
    public IActionResult Comprobante(bool enviado, string? email, string? msg)
    {
        return View(new VotantesComprobanteVm
        {
            PapeletaEnviada = enviado,
            EmailEnmascarado = email,
            Mensaje = msg
        });
    }
}

