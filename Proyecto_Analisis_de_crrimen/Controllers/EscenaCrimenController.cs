using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;
using Proyecto_Analisis_de_crimen.Attributes;
using Proyecto_Analisis_de_crimen.Repositories;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    /// <summary>
    /// Controlador principal: maneja registro, listado, comparación y búsqueda de escenas de crímenes.
    /// Incluye dashboard administrativo con estadísticas.
    /// Aplica DIP: Depende de interfaces, no de implementaciones concretas
    /// </summary>
    public class EscenaCrimenController : Controller
    {
        private readonly IEscenaCrimenService _escenaCrimenService;
        private readonly IComparacionService _comparacionService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor con inyección de dependencias (DIP)
        /// </summary>
        public EscenaCrimenController(
            IEscenaCrimenService escenaCrimenService, 
            IComparacionService comparacionService,
            IUnitOfWork unitOfWork)
        {
            _escenaCrimenService = escenaCrimenService;
            _comparacionService = comparacionService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lista todas las escenas ordenadas por fecha de registro (más recientes primero).
        /// Carga relaciones con Include para evitar consultas N+1.
        /// </summary>
        [RequireAuth]
        public async Task<IActionResult> Index()
        {
            var escenas = await _escenaCrimenService.ObtenerTodasAsync();
            return View(escenas);
        }

        /// <summary>
        /// Muestra formulario de registro. Carga catálogos activos en ViewBag para dropdowns.
        /// </summary>
        [RequireAuth]
        public async Task<IActionResult> Registrar()
        {
            await CargarCatalogosEnViewBag();
            return View();
        }

        /// <summary>
        /// Carga catálogos activos (Tipos de Crimen y Modus Operandi) ordenados alfabéticamente en ViewBag.
        /// </summary>
        private async Task CargarCatalogosEnViewBag()
        {
            var tiposCrimen = await _escenaCrimenService.ObtenerTiposCrimenActivosAsync();
            ViewBag.TiposCrimen = new SelectList(
                tiposCrimen.OrderBy(t => t.Nombre),
                "Id", "Nombre"
            );

            var modusOperandi = await _escenaCrimenService.ObtenerModusOperandiActivosAsync();
            ViewBag.ModusOperandi = new SelectList(
                modusOperandi.OrderBy(m => m.Nombre),
                "Id", "Nombre"
            );
        }

        /// <summary>
        /// Procesa el formulario de registro. Valida datos, verifica catálogos activos,
        /// procesa evidencias y guarda en BD. Maneja errores de BD y genéricos.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Registrar(EscenaCrimen escena, string[] evidencias)
        {
            if (escena.Evidencias == null)
                escena.Evidencias = new List<Evidencia>();

            // Validaciones back-end (el front-end puede ser manipulado)
            if (string.IsNullOrWhiteSpace(escena.Ubicacion))
                ModelState.AddModelError("Ubicacion", "La ubicación es obligatoria");

            // Validar TipoCrimen
            if (escena.TipoCrimenId > 0)
            {
                var tipoCrimen = await _unitOfWork.TiposCrimen.GetByIdAsync(escena.TipoCrimenId);
                if (tipoCrimen == null || !tipoCrimen.Activo)
                    ModelState.AddModelError("TipoCrimenId", "El tipo de crimen seleccionado no existe o no está activo");
            }
            else
                ModelState.AddModelError("TipoCrimenId", "Debe seleccionar un tipo de crimen");

            // Validar ModusOperandi
            if (escena.ModusOperandiId > 0)
            {
                var modusOperandi = await _unitOfWork.ModusOperandi.GetByIdAsync(escena.ModusOperandiId);
                if (modusOperandi == null || !modusOperandi.Activo)
                    ModelState.AddModelError("ModusOperandiId", "El modus operandi seleccionado no existe o no está activo");
            }
            else
                ModelState.AddModelError("ModusOperandiId", "Debe seleccionar un modus operandi");

            if (ModelState.IsValid)
            {
                try
                {
                    // Procesar evidencias (array de strings -> objetos Evidencia)
                    if (escena.Evidencias == null)
                        escena.Evidencias = new List<Evidencia>();
                    escena.Evidencias.Clear();

                    if (evidencias != null && evidencias.Length > 0)
                    {
                        foreach (var evidenciaStr in evidencias)
                        {
                            if (!string.IsNullOrWhiteSpace(evidenciaStr) && 
                                Enum.TryParse<TipoEvidencia>(evidenciaStr, out var tipoEvidencia))
                            {
                                escena.Evidencias.Add(new Evidencia
                                {
                                    TipoEvidencia = tipoEvidencia,
                                    Descripcion = null
                                });
                            }
                        }
                    }

                    // Usar servicio para registrar (mantiene la lógica de negocio separada)
                    var usuarioRegistro = User.Identity?.Name ?? "Sistema";
                    await _escenaCrimenService.RegistrarEscenaAsync(escena, usuarioRegistro);

                    TempData["Success"] = "Escena registrada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    ModelState.AddModelError("", "Error al guardar en la base de datos: " + dbEx.Message);
                    if (dbEx.InnerException != null)
                        ModelState.AddModelError("", "Detalles: " + dbEx.InnerException.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            await CargarCatalogosEnViewBag();
            return View(escena);
        }

        /// <summary>
        /// Muestra formulario de comparación con dropdowns de escenas disponibles.
        /// Formato: "Ubicación - Tipo de Crimen (Fecha)"
        /// </summary>
        [RequireAuth]
        public async Task<IActionResult> Comparar()
        {
            var escenas = await _escenaCrimenService.ObtenerTodasAsync();

            var escenasList = escenas.Select(e => new
            {
                Id = e.Id,
                DisplayText = $"{e.Ubicacion} - {e.TipoCrimen?.Nombre} ({e.FechaCrimen:dd/MM/yyyy})"
            }).ToList();

            ViewBag.Escenas = new SelectList(escenasList, "Id", "DisplayText");
            return View();
        }

        /// <summary>
        /// Realiza comparación entre dos escenas usando ComparacionService.
        /// Valida que no sean la misma escena y que ambas existan.
        /// </summary>
        [HttpPost]
        [RequireAuth]
        public async Task<IActionResult> RealizarComparacion(int escenaBaseId, int escenaComparadaId)
        {
            if (escenaBaseId == escenaComparadaId)
            {
                TempData["Error"] = "No se puede comparar una escena consigo misma. Por favor seleccione dos escenas diferentes.";
                return RedirectToAction(nameof(Comparar));
            }

            var escenaBase = await _escenaCrimenService.ObtenerPorIdAsync(escenaBaseId);
            var escenaComparada = await _escenaCrimenService.ObtenerPorIdAsync(escenaComparadaId);

            if (escenaBase == null || escenaComparada == null)
            {
                TempData["Error"] = "Una o ambas escenas no fueron encontradas";
                return RedirectToAction(nameof(Comparar));
            }

            var resultado = _comparacionService.CompararEscenas(escenaBase, escenaComparada);
            return View("ResultadoComparacion", resultado);
        }

        /// <summary>
        /// Busca escenas similares a una escena base (≥60% similitud).
        /// Retorna resultados ordenados por similitud descendente.
        /// </summary>
        [RequireAuth]
        public async Task<IActionResult> Resultados(int id)
        {
            var escenaBase = await _escenaCrimenService.ObtenerPorIdAsync(id);

            if (escenaBase == null)
                return NotFound();

            var todasLasEscenas = (await _escenaCrimenService.ObtenerTodasAsync()).ToList();
            
            var resultados = _comparacionService.BuscarEscenasSimilares(escenaBase, todasLasEscenas);
            ViewBag.EscenaBase = escenaBase;
            return View(resultados);
        }

        /// <summary>
        /// Dashboard administrativo: muestra total de escenas, crímenes en serie detectados
        /// y últimas 5 escenas registradas. Solo accesible para administradores.
        /// </summary>
        [RequireAdmin]
        public async Task<IActionResult> Dashboard()
        {
            var totalEscenas = await _escenaCrimenService.ObtenerTotalEscenasAsync();
            
            var todasLasEscenas = (await _escenaCrimenService.ObtenerTodasAsync()).ToList();
            
            var crimenesEnSerie = _comparacionService.DetectarCrimenesEnSerie(todasLasEscenas);

            ViewBag.TotalEscenas = totalEscenas;
            ViewBag.CrimenesEnSerie = crimenesEnSerie.Count;
            
            ViewBag.UltimasEscenas = await _escenaCrimenService.ObtenerUltimasEscenasAsync(5);

            return View();
        }
    }
}