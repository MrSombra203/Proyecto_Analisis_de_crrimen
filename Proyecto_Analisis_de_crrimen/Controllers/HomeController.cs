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
            // La página de inicio es pública, pero mostrará diferentes opciones según el estado de autenticación
            // No redirigir automáticamente - dejar que el usuario elija desde la página de inicio
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