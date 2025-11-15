using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;
using Proyecto_Analisis_de_crimen.Attributes;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    public class EscenaCrimenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ComparacionService _comparacionService;

        public EscenaCrimenController(ApplicationDbContext context, ComparacionService comparacionService)
        {
            _context = context;
            _comparacionService = comparacionService;
        }

        // GET: EscenaCrimen
        public async Task<IActionResult> Index()
        {
            var escenas = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();
            return View(escenas);
        }

        // GET: EscenaCrimen/Registrar
        public async Task<IActionResult> Registrar()
        {
            await CargarCatalogosEnViewBag();
            return View();
        }

        private async Task CargarCatalogosEnViewBag()
        {
            ViewBag.TiposCrimen = new SelectList(
                await _context.TiposCrimen.Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync(),
                "Id",
                "Nombre"
            );

            ViewBag.ModusOperandi = new SelectList(
                await _context.ModusOperandi.Where(m => m.Activo).OrderBy(m => m.Nombre).ToListAsync(),
                "Id",
                "Nombre"
            );
        }

        // POST: EscenaCrimen/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(EscenaCrimen escena, string[] evidencias)
        {
            // Asegurar que la colección de evidencias esté inicializada
            if (escena.Evidencias == null)
            {
                escena.Evidencias = new List<Evidencia>();
            }

            // Validar campos requeridos manualmente si es necesario
            if (string.IsNullOrWhiteSpace(escena.Ubicacion))
            {
                ModelState.AddModelError("Ubicacion", "La ubicación es obligatoria");
            }

            // VALIDACIÓN BACK-END: Verificar que TipoCrimenId existe y está activo en la BD
            if (escena.TipoCrimenId > 0)
            {
                var tipoCrimenExiste = await _context.TiposCrimen
                    .AnyAsync(t => t.Id == escena.TipoCrimenId && t.Activo);
                if (!tipoCrimenExiste)
                {
                    ModelState.AddModelError("TipoCrimenId", "El tipo de crimen seleccionado no existe o no está activo");
                }
            }
            else
            {
                ModelState.AddModelError("TipoCrimenId", "Debe seleccionar un tipo de crimen");
            }

            // VALIDACIÓN BACK-END: Verificar que ModusOperandiId existe y está activo en la BD
            if (escena.ModusOperandiId > 0)
            {
                var modusOperandiExiste = await _context.ModusOperandi
                    .AnyAsync(m => m.Id == escena.ModusOperandiId && m.Activo);
                if (!modusOperandiExiste)
                {
                    ModelState.AddModelError("ModusOperandiId", "El modus operandi seleccionado no existe o no está activo");
                }
            }
            else
            {
                ModelState.AddModelError("ModusOperandiId", "Debe seleccionar un modus operandi");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Establecer valores por defecto
                    escena.FechaRegistro = DateTime.Now;
                    escena.UsuarioRegistro = User.Identity?.Name ?? "Sistema";
                    
                    // Limpiar evidencias existentes para evitar duplicados
                    if (escena.Evidencias == null)
                    {
                        escena.Evidencias = new List<Evidencia>();
                    }
                    escena.Evidencias.Clear();

                    // Agregar evidencias seleccionadas
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

                    _context.EscenasCrimen.Add(escena);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Escena registrada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    ModelState.AddModelError("", "Error al guardar en la base de datos: " + dbEx.Message);
                    if (dbEx.InnerException != null)
                    {
                        ModelState.AddModelError("", "Detalles: " + dbEx.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            await CargarCatalogosEnViewBag();
            return View(escena);
        }

        // GET: EscenaCrimen/Comparar
        public async Task<IActionResult> Comparar()
        {
            var escenas = await _context.EscenasCrimen
                .Include(e => e.TipoCrimen)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();

            var escenasList = escenas.Select(e => new
            {
                Id = e.Id,
                DisplayText = $"{e.Ubicacion} - {e.TipoCrimen?.Nombre} ({e.FechaCrimen:dd/MM/yyyy})"
            }).ToList();

            ViewBag.Escenas = new SelectList(escenasList, "Id", "DisplayText");
            return View();
        }

        // POST: EscenaCrimen/RealizarComparacion
        [HttpPost]
        public async Task<IActionResult> RealizarComparacion(int escenaBaseId, int escenaComparadaId)
        {
            var escenaBase = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .FirstOrDefaultAsync(e => e.Id == escenaBaseId);

            var escenaComparada = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .FirstOrDefaultAsync(e => e.Id == escenaComparadaId);

            if (escenaBase == null || escenaComparada == null)
            {
                TempData["Error"] = "Una o ambas escenas no fueron encontradas";
                return RedirectToAction(nameof(Comparar));
            }

            var resultado = _comparacionService.CompararEscenas(escenaBase, escenaComparada);
            return View("ResultadoComparacion", resultado);
        }

        // GET: EscenaCrimen/Resultados/{id}
        public async Task<IActionResult> Resultados(int id)
        {
            var escenaBase = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (escenaBase == null)
            {
                return NotFound();
            }

            var todasLasEscenas = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .ToListAsync();
            var resultados = _comparacionService.BuscarEscenasSimilares(escenaBase, todasLasEscenas);

            ViewBag.EscenaBase = escenaBase;
            return View(resultados);
        }

        // GET: EscenaCrimen/Dashboard
        [RequireAdmin] // Solo administradores pueden acceder al dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalEscenas = await _context.EscenasCrimen.CountAsync();
            var todasLasEscenas = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .ToListAsync();
            var crimenesEnSerie = _comparacionService.DetectarCrimenesEnSerie(todasLasEscenas);

            ViewBag.TotalEscenas = totalEscenas;
            ViewBag.CrimenesEnSerie = crimenesEnSerie.Count;
            ViewBag.UltimasEscenas = await _context.EscenasCrimen
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .OrderByDescending(e => e.FechaRegistro)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}