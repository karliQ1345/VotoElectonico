using Microsoft.AspNetCore.Mvc;

namespace VotoElect.MVC.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}

