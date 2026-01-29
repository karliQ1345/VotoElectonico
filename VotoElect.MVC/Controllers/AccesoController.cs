using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.Services;
using VotoElect.MVC.ViewModels;

namespace VotoElect.MVC.Controllers;

public class AccesoController : Controller
{
    private readonly ApiService _api;

    public AccesoController(ApiService api) => _api = api;

    [HttpGet]
    public IActionResult Index() => View(new AccesoIndexVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string cedula, CancellationToken ct)
    {
        var vm = new AccesoIndexVm { Cedula = (cedula ?? "").Trim() };

        if (string.IsNullOrWhiteSpace(vm.Cedula))
        {
            vm.Error = "Ingrese su cédula.";
            return View(vm);
        }

        var resp = await _api.LoginAsync(vm.Cedula, ct);
        if (resp == null || !resp.Ok || resp.Data == null)
        {
            vm.Error = resp?.Message ?? "No se pudo validar la cédula.";
            return View(vm);
        }

        HttpContext.Session.SetString("rol", resp.Data.RolPrincipal ?? "");
        HttpContext.Session.SetString("redirect", resp.Data.Redirect ?? "");

        if (resp.Data.RequiereTwoFactor)
        {
            if (!string.IsNullOrWhiteSpace(resp.Data.TwoFactorSessionId))
                HttpContext.Session.SetString("twoFactorSessionId", resp.Data.TwoFactorSessionId);

            HttpContext.Session.SetString("emailMask", resp.Data.EmailEnmascarado ?? "");
            return RedirectToAction(nameof(Otp));
        }


        // Si no requiere OTP (votante) → ir a pedir código
        return RedirectToAction("Codigo", "Votantes");
    }

    [HttpGet]
    public IActionResult Otp()
    {
        var vm = new AccesoOtpVm
        {
            EmailEnmascarado = HttpContext.Session.GetString("emailMask")
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Otp(string codigo, CancellationToken ct)
    {
        var vm = new AccesoOtpVm { Codigo = (codigo ?? "").Trim() };

        var sessionId = HttpContext.Session.GetString("twoFactorSessionId");
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            vm.Error = "Sesión OTP expirada. Vuelva a ingresar su cédula.";
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Codigo))
        {
            vm.Error = "Ingrese el código OTP.";
            return View(vm);
        }

        var resp = await _api.VerifyOtpAsync(sessionId, vm.Codigo, ct);
        if (resp == null || !resp.Ok || resp.Data == null)
        {
            vm.Error = resp?.Message ?? "OTP inválido.";
            return View(vm);
        }

        // Guardar token y redirigir por rol
        HttpContext.Session.SetString("token", resp.Data.Token ?? "");

        var rol = HttpContext.Session.GetString("rol") ?? "";
        if (rol.Contains("JefeJunta", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Panel", "JefeJunta");

        if (rol.Contains("Admin", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Dashboard", "Admin");

        // fallback
        return RedirectToAction("Codigo", "Votantes");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}


