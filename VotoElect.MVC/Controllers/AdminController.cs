using Microsoft.AspNetCore.Mvc;
using VotoElect.MVC.Services;

namespace VotoElectronico.MVC.Controllers;

public class AdminController : Controller
{
    private string? Rol() => HttpContext.Session.GetString(SessionKeys.Rol);

    [HttpGet]
    public IActionResult Dashboard()
    {
        if (Rol() != "Administrador") return RedirectToAction("Denegado", "Acceso");
        return View();
    }

    [HttpGet]
    public IActionResult Resultados()
    {
        if (Rol() != "Administrador") return RedirectToAction("Denegado", "Acceso");
        return View();
    }
}

