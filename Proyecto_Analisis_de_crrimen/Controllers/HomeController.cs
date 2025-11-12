using Microsoft.AspNetCore.Mvc;
using System.Web.Mvc;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Sistema de Análisis de Escenas del Crimen";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contacto - Soporte Técnico";
            return View();
        }
    }
}