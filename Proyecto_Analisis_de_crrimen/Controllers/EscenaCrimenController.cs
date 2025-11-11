using Microsoft.AspNetCore.Mvc;

namespace Proyecto_Analisis_de_crrimen.Controllers
{
    public class EscenaCrimenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
