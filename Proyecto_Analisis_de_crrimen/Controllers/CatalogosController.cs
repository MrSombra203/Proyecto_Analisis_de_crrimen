using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Attributes;
using Proyecto_Analisis_de_crimen.Services;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    // Controlador para gestionar los catálogos del sistema (tipos de crimen y modus operandi)
    // Solo los administradores pueden acceder aquí
    // Aplica DIP: Depende de interfaces, no de implementaciones concretas
    [RequireAdmin]
    public class CatalogosController : Controller
    {
        private readonly ICatalogoService _catalogoService;

        // Constructor con inyección de dependencias (DIP)
        public CatalogosController(ICatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        // ============================================
        // TIPOS DE CRIMEN
        // ============================================

        // Muestra la lista de todos los tipos de crimen, ordenados alfabéticamente
        public async Task<IActionResult> TiposCrimen()
        {
            var tipos = (await _catalogoService.ObtenerTiposCrimenAsync()).OrderBy(t => t.Nombre);
            return View(tipos);
        }

        // Muestra el formulario para crear un nuevo tipo de crimen
        public IActionResult CrearTipoCrimen()
        {
            return View();
        }

        // Procesa el formulario cuando se crea un nuevo tipo de crimen
        // Le asigna automáticamente la fecha de creación y lo marca como activo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTipoCrimen(TipoCrimen tipoCrimen)
        {
            // Validar unicidad
            if (await _catalogoService.TipoCrimenExisteAsync(tipoCrimen.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe un tipo de crimen con este nombre");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _catalogoService.CrearTipoCrimenAsync(tipoCrimen);
                    TempData["Success"] = $"Tipo de crimen '{tipoCrimen.Nombre}' creado exitosamente";
                    return RedirectToAction(nameof(TiposCrimen));
                }
                catch (DbUpdateException dbEx)
                {
                    ManejarErrorUnicidad(dbEx, "tipo de crimen", "guardar");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(tipoCrimen);
        }

        // Muestra el formulario de edición con los datos del tipo de crimen
        public async Task<IActionResult> EditarTipoCrimen(int id)
        {
            var tipoCrimen = await _catalogoService.ObtenerTipoCrimenPorIdAsync(id);
            if (tipoCrimen == null)
            {
                return NotFound();
            }
            return View(tipoCrimen);
        }

        // Procesa el formulario cuando se edita un tipo de crimen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTipoCrimen(int id, TipoCrimen tipoCrimen)
        {
            // Verificamos que el ID coincida
            if (id != tipoCrimen.Id)
            {
                return NotFound();
            }

            // Validar unicidad excluyendo el registro actual
            if (await _catalogoService.TipoCrimenExisteAsync(tipoCrimen.Nombre, id))
            {
                ModelState.AddModelError("Nombre", "Ya existe un tipo de crimen con este nombre");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _catalogoService.ActualizarTipoCrimenAsync(tipoCrimen);
                    TempData["Success"] = $"Tipo de crimen '{tipoCrimen.Nombre}' actualizado exitosamente";
                    return RedirectToAction(nameof(TiposCrimen));
                }
                catch (DbUpdateException dbEx)
                {
                    ManejarErrorUnicidad(dbEx, "tipo de crimen", "actualizar");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(tipoCrimen);
        }

        // Activa o desactiva un tipo de crimen (cambia su estado)
        // Si está desactivado, no aparecerá en los dropdowns
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarTipoCrimen(int id)
        {
            var tipoCrimen = await _catalogoService.ObtenerTipoCrimenPorIdAsync(id);
            if (tipoCrimen == null)
            {
                return NotFound();
            }

            var exito = await _catalogoService.CambiarEstadoTipoCrimenAsync(id);
            if (exito)
            {
                string estado = tipoCrimen.Activo ? "desactivado" : "activado";
                TempData["Success"] = $"Tipo de crimen '{tipoCrimen.Nombre}' {estado} exitosamente";
            }
            
            return RedirectToAction(nameof(TiposCrimen));
        }

        // ============================================
        // MODUS OPERANDI
        // ============================================

        // Muestra la lista de todos los modus operandi, ordenados alfabéticamente
        public async Task<IActionResult> ModusOperandi()
        {
            var modus = (await _catalogoService.ObtenerModusOperandiAsync()).OrderBy(m => m.Nombre);
            return View(modus);
        }

        // Muestra el formulario para crear un nuevo modus operandi
        public IActionResult CrearModusOperandi()
        {
            return View();
        }

        // Procesa el formulario cuando se crea un nuevo modus operandi
        // Le asigna automáticamente la fecha de creación y lo marca como activo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearModusOperandi(ModusOperandi modusOperandi)
        {
            // Validar unicidad
            if (await _catalogoService.ModusOperandiExisteAsync(modusOperandi.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe un modus operandi con este nombre");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _catalogoService.CrearModusOperandiAsync(modusOperandi);
                    TempData["Success"] = $"Modus operandi '{modusOperandi.Nombre}' creado exitosamente";
                    return RedirectToAction(nameof(ModusOperandi));
                }
                catch (DbUpdateException dbEx)
                {
                    ManejarErrorUnicidad(dbEx, "modus operandi", "guardar");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(modusOperandi);
        }

        // Muestra el formulario de edición con los datos del modus operandi
        public async Task<IActionResult> EditarModusOperandi(int id)
        {
            var modusOperandi = await _catalogoService.ObtenerModusOperandiPorIdAsync(id);
            if (modusOperandi == null)
            {
                return NotFound();
            }
            return View(modusOperandi);
        }

        // Procesa el formulario cuando se edita un modus operandi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarModusOperandi(int id, ModusOperandi modusOperandi)
        {
            // Verificamos que el ID coincida
            if (id != modusOperandi.Id)
            {
                return NotFound();
            }

            // Validar unicidad excluyendo el registro actual
            if (await _catalogoService.ModusOperandiExisteAsync(modusOperandi.Nombre, id))
            {
                ModelState.AddModelError("Nombre", "Ya existe un modus operandi con este nombre");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _catalogoService.ActualizarModusOperandiAsync(modusOperandi);
                    TempData["Success"] = $"Modus operandi '{modusOperandi.Nombre}' actualizado exitosamente";
                    return RedirectToAction(nameof(ModusOperandi));
                }
                catch (DbUpdateException dbEx)
                {
                    ManejarErrorUnicidad(dbEx, "modus operandi", "actualizar");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(modusOperandi);
        }

        // Activa o desactiva un modus operandi (cambia su estado)
        // Si está desactivado, no aparecerá en los dropdowns
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarModusOperandi(int id)
        {
            var modusOperandi = await _catalogoService.ObtenerModusOperandiPorIdAsync(id);
            if (modusOperandi == null)
            {
                return NotFound();
            }

            var exito = await _catalogoService.CambiarEstadoModusOperandiAsync(id);
            if (exito)
            {
                string estado = modusOperandi.Activo ? "desactivado" : "activado";
                TempData["Success"] = $"Modus operandi '{modusOperandi.Nombre}' {estado} exitosamente";
            }
            
            return RedirectToAction(nameof(ModusOperandi));
        }

        // ============================================
        // Métodos auxiliares
        // ============================================

        // Maneja los errores cuando se intenta crear o actualizar algo con un nombre que ya existe
        // Detecta si el error es por violación de la constraint UNIQUE de la base de datos
        private void ManejarErrorUnicidad(DbUpdateException dbEx, string entidad, string operacion)
        {
            // Si el error es porque ya existe un registro con ese nombre
            if (dbEx.InnerException?.Message.Contains("UNIQUE") == true)
            {
                ModelState.AddModelError("Nombre", $"Ya existe un {entidad} con este nombre");
            }
            else
            {
                ModelState.AddModelError("", $"Error al {operacion}: {dbEx.Message}");
            }
        }
    }
}

