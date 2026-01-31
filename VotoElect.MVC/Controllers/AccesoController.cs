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

        HttpContext.Session.SetString(SessionKeys.Cedula, vm.Cedula);
        HttpContext.Session.SetString(SessionKeys.Rol, resp.Data.RolPrincipal ?? "");
        HttpContext.Session.SetString("REDIRECT", resp.Data.Redirect ?? "");
        HttpContext.Session.SetString(SessionKeys.EmailMasked, resp.Data.EmailEnmascarado ?? "");

        if (resp.Data.RequiereTwoFactor)
        {
            if (!string.IsNullOrWhiteSpace(resp.Data.TwoFactorSessionId))
                HttpContext.Session.SetString(SessionKeys.TwoFactorSessionId, resp.Data.TwoFactorSessionId);

            return RedirectToAction(nameof(Otp));
        }

        return RedirectToAction("Codigo", "Votantes");
    }

    [HttpGet]
    public IActionResult Otp()
    {
        var vm = new AccesoOtpVm
        {
            EmailEnmascarado = HttpContext.Session.GetString(SessionKeys.EmailMasked) ?? ""
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Otp(string codigo, CancellationToken ct)
    {
        var vm = new AccesoOtpVm
        {
            Codigo = (codigo ?? "").Trim(),
            EmailEnmascarado = HttpContext.Session.GetString(SessionKeys.EmailMasked) ?? ""
        };

        var sessionId = HttpContext.Session.GetString(SessionKeys.TwoFactorSessionId);
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
        if (resp == null || !resp.Ok || resp.Data == null || !resp.Data.Verificado)
        {
            vm.Error = resp?.Message ?? "OTP inválido.";
            return View(vm);
        }

        HttpContext.Session.SetString(SessionKeys.Token, resp.Data.Token ?? "");
        HttpContext.Session.SetString(SessionKeys.Rol, resp.Data.RolPrincipal ?? "");

        
        HttpContext.Session.Remove(SessionKeys.TwoFactorSessionId);

        var rol = (resp.Data.RolPrincipal ?? "").Trim();

        if (rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
            rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Procesos", "Admin");
        }

        if (rol.Equals("JefeJunta", StringComparison.OrdinalIgnoreCase) ||
            rol.Equals("Jefe de Junta", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Panel", "JefeJunta");
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}




