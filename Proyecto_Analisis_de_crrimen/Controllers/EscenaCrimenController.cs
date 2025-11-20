using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;
using Proyecto_Analisis_de_crimen.Attributes;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    /// <summary>
    /// Controlador principal del sistema que maneja todas las operaciones relacionadas
    /// con las escenas de crímenes.
    /// 
    /// Responsabilidades:
    /// - Registro de nuevas escenas de crímenes
    /// - Listado y visualización de escenas
    /// - Comparación entre escenas
    /// - Búsqueda de escenas similares
    /// - Dashboard administrativo con estadísticas
    /// </summary>
    public class EscenaCrimenController : Controller
    {
        // Inyección de dependencias: contexto de base de datos y servicio de comparación
        private readonly ApplicationDbContext _context;
        private readonly ComparacionService _comparacionService;

        /// <summary>
        /// Constructor que recibe las dependencias mediante inyección de dependencias (DI)
        /// </summary>
        public EscenaCrimenController(ApplicationDbContext context, ComparacionService comparacionService)
        {
            _context = context;
            _comparacionService = comparacionService;
        }

        /// <summary>
        /// Muestra la lista de todas las escenas de crímenes registradas en el sistema.
        /// 
        /// Esta acción:
        /// 1. Carga todas las escenas con sus relaciones (Evidencias, TipoCrimen, ModusOperandi)
        /// 2. Las ordena por fecha de registro descendente (más recientes primero)
        /// 3. Las envía a la vista para su visualización
        /// 
        /// Requiere autenticación: [RequireAuth]
        /// </summary>
        /// <returns>Vista con la lista de escenas</returns>
        [RequireAuth]
        public async Task<IActionResult> Index()
        {
            // Cargar escenas con todas sus relaciones usando Include (Eager Loading)
            // Esto evita el problema N+1 de consultas a la base de datos
            var escenas = await _context.EscenasCrimen
                .Include(e => e.Evidencias)        // Cargar evidencias relacionadas
                .Include(e => e.TipoCrimen)        // Cargar tipo de crimen relacionado
                .Include(e => e.ModusOperandi)     // Cargar modus operandi relacionado
                .OrderByDescending(e => e.FechaRegistro)  // Ordenar: más recientes primero
                .ToListAsync();
            
            return View(escenas);
        }

        /// <summary>
        /// Muestra el formulario para registrar una nueva escena de crimen.
        /// 
        /// Esta acción carga los catálogos necesarios (Tipos de Crimen y Modus Operandi)
        /// en el ViewBag para poblar los dropdowns del formulario.
        /// 
        /// Requiere autenticación: [RequireAuth]
        /// </summary>
        /// <returns>Vista del formulario de registro</returns>
        [RequireAuth]
        public async Task<IActionResult> Registrar()
        {
            // Cargar los catálogos necesarios para los dropdowns del formulario
            await CargarCatalogosEnViewBag();
            return View();
        }

        /// <summary>
        /// Método auxiliar que carga los catálogos activos en el ViewBag.
        /// 
        /// ViewBag es un diccionario dinámico que permite pasar datos del controlador
        /// a la vista. Aquí se cargan:
        /// - Tipos de Crimen activos (para dropdown)
        /// - Modus Operandi activos (para dropdown)
        /// 
        /// Se filtran solo los registros activos y se ordenan alfabéticamente.
        /// </summary>
        private async Task CargarCatalogosEnViewBag()
        {
            // Cargar tipos de crimen activos y convertirlos a SelectList para dropdowns
            ViewBag.TiposCrimen = new SelectList(
                await _context.TiposCrimen
                    .Where(t => t.Activo)              // Solo activos
                    .OrderBy(t => t.Nombre)            // Orden alfabético
                    .ToListAsync(),
                "Id",      // Valor del option (ID del tipo)
                "Nombre"   // Texto visible del option
            );

            // Cargar modus operandi activos y convertirlos a SelectList para dropdowns
            ViewBag.ModusOperandi = new SelectList(
                await _context.ModusOperandi
                    .Where(m => m.Activo)              // Solo activos
                    .OrderBy(m => m.Nombre)            // Orden alfabético
                    .ToListAsync(),
                "Id",      // Valor del option (ID del modus operandi)
                "Nombre"   // Texto visible del option
            );
        }

        /// <summary>
        /// Procesa el formulario de registro de una nueva escena de crimen.
        /// 
        /// Este método:
        /// 1. Valida los datos del formulario (front-end y back-end)
        /// 2. Verifica que los catálogos seleccionados existan y estén activos
        /// 3. Procesa las evidencias seleccionadas
        /// 4. Guarda la escena en la base de datos
        /// 5. Maneja errores y muestra mensajes apropiados
        /// 
        /// Validaciones de seguridad:
        /// - [HttpPost]: Solo acepta peticiones POST
        /// - [ValidateAntiForgeryToken]: Protección CSRF (Cross-Site Request Forgery)
        /// - [RequireAuth]: Requiere usuario autenticado
        /// </summary>
        /// <param name="escena">Objeto EscenaCrimen con los datos del formulario</param>
        /// <param name="evidencias">Array de strings con los tipos de evidencias seleccionadas</param>
        /// <returns>Redirige a Index si es exitoso, o retorna la vista con errores</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]  // Protección contra ataques CSRF
        [RequireAuth]
        public async Task<IActionResult> Registrar(EscenaCrimen escena, string[] evidencias)
        {
            // Inicializar la colección de evidencias si es null
            // Esto previene errores de NullReferenceException
            if (escena.Evidencias == null)
            {
                escena.Evidencias = new List<Evidencia>();
            }

            // ============================================
            // VALIDACIONES BACK-END
            // ============================================
            // Las validaciones back-end son críticas porque el front-end puede ser manipulado.
            // Siempre validar en el servidor, nunca confiar solo en el cliente.

            // Validar que la ubicación no esté vacía
            if (string.IsNullOrWhiteSpace(escena.Ubicacion))
            {
                ModelState.AddModelError("Ubicacion", "La ubicación es obligatoria");
            }

            // Validar que el TipoCrimenId existe y está activo en la base de datos
            // Esto previene que se inserten datos inválidos o referencias a registros desactivados
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

            // Validar que el ModusOperandiId existe y está activo en la base de datos
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

            // Si todas las validaciones pasaron, proceder a guardar
            if (ModelState.IsValid)
            {
                try
                {
                    // ============================================
                    // PREPARACIÓN DE DATOS
                    // ============================================
                    
                    // Establecer valores por defecto automáticos
                    escena.FechaRegistro = DateTime.Now;  // Fecha/hora actual del servidor
                    escena.UsuarioRegistro = User.Identity?.Name ?? "Sistema";  // Usuario que registra
                    
                    // Limpiar evidencias existentes para evitar duplicados
                    // (pueden venir del binding del modelo)
                    if (escena.Evidencias == null)
                    {
                        escena.Evidencias = new List<Evidencia>();
                    }
                    escena.Evidencias.Clear();

                    // ============================================
                    // PROCESAMIENTO DE EVIDENCIAS
                    // ============================================
                    // Las evidencias vienen como array de strings desde el formulario
                    // Necesitamos convertirlas a objetos Evidencia
                    if (evidencias != null && evidencias.Length > 0)
                    {
                        foreach (var evidenciaStr in evidencias)
                        {
                            // Validar que el string no esté vacío y pueda convertirse al enum
                            if (!string.IsNullOrWhiteSpace(evidenciaStr) && 
                                Enum.TryParse<TipoEvidencia>(evidenciaStr, out var tipoEvidencia))
                            {
                                // Crear nueva evidencia y agregarla a la colección
                                escena.Evidencias.Add(new Evidencia
                                {
                                    TipoEvidencia = tipoEvidencia,
                                    Descripcion = null  // La descripción se puede agregar después
                                });
                            }
                        }
                    }

                    // ============================================
                    // GUARDAR EN BASE DE DATOS
                    // ============================================
                    _context.EscenasCrimen.Add(escena);  // Marcar para inserción
                    await _context.SaveChangesAsync();   // Ejecutar INSERT en la BD

                    // Mensaje de éxito que se mostrará en la siguiente página
                    TempData["Success"] = "Escena registrada exitosamente";
                    
                    // Redirigir a la lista de escenas
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    // Error específico de base de datos (constraints, claves foráneas, etc.)
                    ModelState.AddModelError("", "Error al guardar en la base de datos: " + dbEx.Message);
                    if (dbEx.InnerException != null)
                    {
                        ModelState.AddModelError("", "Detalles: " + dbEx.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    // Error genérico no esperado
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            // Si hay errores, recargar los catálogos y mostrar el formulario con errores
            await CargarCatalogosEnViewBag();
            return View(escena);
        }

        /// <summary>
        /// Muestra el formulario para comparar dos escenas de crímenes.
        /// 
        /// Esta acción carga todas las escenas disponibles y las prepara
        /// para ser seleccionadas en dos dropdowns (escena base y escena a comparar).
        /// 
        /// Requiere autenticación: [RequireAuth]
        /// </summary>
        /// <returns>Vista con los dropdowns para seleccionar escenas</returns>
        [RequireAuth]
        public async Task<IActionResult> Comparar()
        {
            // Cargar todas las escenas con su tipo de crimen para mostrar en los dropdowns
            var escenas = await _context.EscenasCrimen
                .Include(e => e.TipoCrimen)  // Necesario para mostrar el nombre del tipo
                .OrderByDescending(e => e.FechaRegistro)  // Más recientes primero
                .ToListAsync();

            // Crear una lista anónima con formato legible para los dropdowns
            // Formato: "Ubicación - Tipo de Crimen (Fecha)"
            var escenasList = escenas.Select(e => new
            {
                Id = e.Id,
                DisplayText = $"{e.Ubicacion} - {e.TipoCrimen?.Nombre} ({e.FechaCrimen:dd/MM/yyyy})"
            }).ToList();

            // Pasar las escenas al ViewBag para poblar los SelectList en la vista
            ViewBag.Escenas = new SelectList(escenasList, "Id", "DisplayText");
            return View();
        }

        /// <summary>
        /// Realiza la comparación entre dos escenas seleccionadas.
        /// 
        /// Este método:
        /// 1. Carga ambas escenas con todas sus relaciones desde la BD
        /// 2. Valida que ambas escenas existan
        /// 3. Utiliza el ComparacionService para calcular la similitud
        /// 4. Muestra el resultado de la comparación
        /// 
        /// Requiere autenticación: [RequireAuth]
        /// </summary>
        /// <param name="escenaBaseId">ID de la escena de referencia</param>
        /// <param name="escenaComparadaId">ID de la escena a comparar</param>
        /// <returns>Vista con el resultado detallado de la comparación</returns>
        [HttpPost]
        [RequireAuth]
        public async Task<IActionResult> RealizarComparacion(int escenaBaseId, int escenaComparadaId)
        {
            // Validar que no se esté comparando la misma escena consigo misma
            if (escenaBaseId == escenaComparadaId)
            {
                TempData["Error"] = "No se puede comparar una escena consigo misma. Por favor seleccione dos escenas diferentes.";
                return RedirectToAction(nameof(Comparar));
            }

            // Cargar la escena base con todas sus relaciones necesarias para la comparación
            var escenaBase = await _context.EscenasCrimen
                .Include(e => e.Evidencias)      // Necesarias para comparar evidencias
                .Include(e => e.TipoCrimen)       // Para mostrar información
                .Include(e => e.ModusOperandi)    // Para mostrar información
                .FirstOrDefaultAsync(e => e.Id == escenaBaseId);

            // Cargar la escena a comparar con todas sus relaciones
            var escenaComparada = await _context.EscenasCrimen
                .Include(e => e.Evidencias)      // Necesarias para comparar evidencias
                .Include(e => e.TipoCrimen)      // Para mostrar información
                .Include(e => e.ModusOperandi)   // Para mostrar información
                .FirstOrDefaultAsync(e => e.Id == escenaComparadaId);

            // Validar que ambas escenas existan
            if (escenaBase == null || escenaComparada == null)
            {
                TempData["Error"] = "Una o ambas escenas no fueron encontradas";
                return RedirectToAction(nameof(Comparar));
            }

            // Utilizar el servicio de comparación (CORE del sistema) para calcular similitud
            var resultado = _comparacionService.CompararEscenas(escenaBase, escenaComparada);
            
            // Mostrar la vista de resultados con el objeto ComparacionResultado
            return View("ResultadoComparacion", resultado);
        }

        /// <summary>
        /// Busca y muestra todas las escenas similares a una escena base específica.
        /// 
        /// Este método:
        /// 1. Carga la escena base por su ID
        /// 2. Carga todas las escenas del sistema
        /// 3. Utiliza el ComparacionService para encontrar escenas similares (≥60% similitud)
        /// 4. Muestra los resultados ordenados por similitud descendente
        /// 
        /// Requiere autenticación: [RequireAuth]
        /// </summary>
        /// <param name="id">ID de la escena base para buscar similares</param>
        /// <returns>Vista con la lista de escenas similares encontradas</returns>
        [RequireAuth]
        public async Task<IActionResult> Resultados(int id)
        {
            // Cargar la escena base con todas sus relaciones
            var escenaBase = await _context.EscenasCrimen
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .FirstOrDefaultAsync(e => e.Id == id);

            // Validar que la escena existe
            if (escenaBase == null)
            {
                return NotFound();  // Retorna 404 si no existe
            }

            // Cargar TODAS las escenas del sistema para comparar
            var todasLasEscenas = await _context.EscenasCrimen
                .Include(e => e.Evidencias)      // Necesarias para comparación
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .ToListAsync();
            
            // Buscar escenas similares usando el servicio (umbral por defecto: 60%)
            var resultados = _comparacionService.BuscarEscenasSimilares(escenaBase, todasLasEscenas);

            // Pasar la escena base al ViewBag para mostrarla en la vista
            ViewBag.EscenaBase = escenaBase;
            return View(resultados);
        }

        /// <summary>
        /// Dashboard administrativo con estadísticas del sistema.
        /// 
        /// Este dashboard muestra:
        /// - Total de escenas registradas
        /// - Número de crímenes en serie detectados
        /// - Últimas 5 escenas registradas
        /// 
        /// Solo accesible para administradores: [RequireAdmin]
        /// </summary>
        /// <returns>Vista del dashboard con estadísticas</returns>
        [RequireAdmin]  // Solo administradores pueden acceder al dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Contar total de escenas registradas en el sistema
            var totalEscenas = await _context.EscenasCrimen.CountAsync();
            
            // Cargar todas las escenas para detectar series criminales
            var todasLasEscenas = await _context.EscenasCrimen
                .Include(e => e.Evidencias)      // Necesarias para comparación
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .ToListAsync();
            
            // Detectar crímenes en serie usando el servicio (umbral: 75%)
            var crimenesEnSerie = _comparacionService.DetectarCrimenesEnSerie(todasLasEscenas);

            // Preparar datos para la vista usando ViewBag
            ViewBag.TotalEscenas = totalEscenas;
            ViewBag.CrimenesEnSerie = crimenesEnSerie.Count;  // Número de grupos de series detectados
            
            // Cargar las últimas 5 escenas registradas para mostrar actividad reciente
            ViewBag.UltimasEscenas = await _context.EscenasCrimen
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .OrderByDescending(e => e.FechaRegistro)  // Más recientes primero
                .Take(5)  // Solo las primeras 5
                .ToListAsync();

            return View();
        }
    }
}