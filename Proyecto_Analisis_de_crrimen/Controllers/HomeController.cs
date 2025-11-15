using Microsoft.AspNetCore.Mvc;
using Proyecto_Analisis_de_crimen.Models;
using System.Diagnostics;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Si el usuario está autenticado y es administrador, redirigir al dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
            var rolId = HttpContext.Session.GetInt32("RolId");
            
            if (userId.HasValue && rolId == 1) // 1 = Administrador
            {
                return RedirectToAction("Dashboard", "EscenaCrimen");
            }
            
            // Si el usuario está autenticado pero no es administrador, redirigir a Index de EscenaCrimen
            if (userId.HasValue)
            {
                return RedirectToAction("Index", "EscenaCrimen");
            }
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}