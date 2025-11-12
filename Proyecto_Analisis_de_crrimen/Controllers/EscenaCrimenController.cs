using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    public class EscenaCrimenController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly ComparacionService _comparacionService;

        public EscenaCrimenController()
        {
            _comparacionService = new ComparacionService();
        }

        // GET: EscenaCrimen
        public ActionResult Index()
        {
            var escenas = db.EscenasCrimen
                .Include(e => e.Evidencias)
                .OrderByDescending(e => e.FechaRegistro)
                .ToList();
            return View(escenas);
        }

        // GET: EscenaCrimen/Registrar
        public ActionResult Registrar()
        {
            return View();
        }

        // POST: EscenaCrimen/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(EscenaCrimen escena, string[] evidencias)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    escena.FechaRegistro = DateTime.Now;
                    escena.UsuarioRegistro = User.Identity.Name ?? "Sistema";

                    if (evidencias != null)
                    {
                        foreach (var evidenciaStr in evidencias)
                        {
                            if (Enum.TryParse<TipoEvidencia>(evidenciaStr, out var tipoEvidencia))
                            {
                                escena.Evidencias.Add(new Evidencia
                                {
                                    TipoEvidencia = tipoEvidencia
                                });
                            }
                        }
                    }

                    db.EscenasCrimen.Add(escena);
                    db.SaveChanges();

                    TempData["Success"] = "Escena registrada exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            return View(escena);
        }

        // GET: EscenaCrimen/Comparar
        public ActionResult Comparar()
        {
            ViewBag.Escenas = new SelectList(db.EscenasCrimen.OrderByDescending(e => e.FechaRegistro), "Id", "Ubicacion");
            return View();
        }

        // POST: EscenaCrimen/RealizarComparacion
        [HttpPost]
        public ActionResult RealizarComparacion(int escenaBaseId, int escenaComparadaId)
        {
            var escenaBase = db.EscenasCrimen.Include(e => e.Evidencias).FirstOrDefault(e => e.Id == escenaBaseId);
            var escenaComparada = db.EscenasCrimen.Include(e => e.Evidencias).FirstOrDefault(e => e.Id == escenaComparadaId);

            if (escenaBase == null || escenaComparada == null)
            {
                TempData["Error"] = "Una o ambas escenas no fueron encontradas";
                return RedirectToAction("Comparar");
            }

            var resultado = _comparacionService.CompararEscenas(escenaBase, escenaComparada);
            return View("ResultadoComparacion", resultado);
        }

        // GET: EscenaCrimen/Resultados/{id}
        public ActionResult Resultados(int id)
        {
            var escenaBase = db.EscenasCrimen.Include(e => e.Evidencias).FirstOrDefault(e => e.Id == id);
            if (escenaBase == null)
            {
                return HttpNotFound();
            }

            var todasLasEscenas = db.EscenasCrimen.Include(e => e.Evidencias).ToList();
            var resultados = _comparacionService.BuscarEscenasSimilares(escenaBase, todasLasEscenas);

            ViewBag.EscenaBase = escenaBase;
            return View(resultados);
        }

        // GET: EscenaCrimen/Dashboard
        public ActionResult Dashboard()
        {
            var totalEscenas = db.EscenasCrimen.Count();
            var todasLasEscenas = db.EscenasCrimen.Include(e => e.Evidencias).ToList();
            var crimenesEnSerie = _comparacionService.DetectarCrimenesEnSerie(todasLasEscenas);

            ViewBag.TotalEscenas = totalEscenas;
            ViewBag.CrimenesEnSerie = crimenesEnSerie.Count;
            ViewBag.UltimasEscenas = db.EscenasCrimen.OrderByDescending(e => e.FechaRegistro).Take(5).ToList();

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}