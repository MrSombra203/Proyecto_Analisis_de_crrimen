using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Attributes;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    // Controlador para gestionar los catálogos del sistema (tipos de crimen y modus operandi)
    // Solo los administradores pueden acceder aquí
    [RequireAdmin]
    public class CatalogosController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor con inyección de dependencias
        public CatalogosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // TIPOS DE CRIMEN
        // ============================================

        // Muestra la lista de todos los tipos de crimen, ordenados alfabéticamente
        public async Task<IActionResult> TiposCrimen()
        {
            var tipos = await _context.TiposCrimen
                .OrderBy(t => t.Nombre)
                .ToListAsync();
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Le ponemos la fecha actual y lo activamos
                    tipoCrimen.FechaCreacion = DateTime.Now;
                    tipoCrimen.Activo = true;

                    _context.TiposCrimen.Add(tipoCrimen);
                    await _context.SaveChangesAsync();

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
            var tipoCrimen = await _context.TiposCrimen.FindAsync(id);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoCrimen);
                    await _context.SaveChangesAsync();

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
            var tipoCrimen = await _context.TiposCrimen.FindAsync(id);
            if (tipoCrimen == null)
            {
                return NotFound();
            }

            // Cambiamos el estado (si estaba activo, lo desactivamos y viceversa)
            tipoCrimen.Activo = !tipoCrimen.Activo;
            await _context.SaveChangesAsync();

            string estado = tipoCrimen.Activo ? "activado" : "desactivado";
            TempData["Success"] = $"Tipo de crimen '{tipoCrimen.Nombre}' {estado} exitosamente";
            
            return RedirectToAction(nameof(TiposCrimen));
        }

        // ============================================
        // MODUS OPERANDI
        // ============================================

        // Muestra la lista de todos los modus operandi, ordenados alfabéticamente
        public async Task<IActionResult> ModusOperandi()
        {
            var modus = await _context.ModusOperandi
                .OrderBy(m => m.Nombre)
                .ToListAsync();
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Configurar valores por defecto
                    modusOperandi.FechaCreacion = DateTime.Now;
                    modusOperandi.Activo = true;

                    _context.ModusOperandi.Add(modusOperandi);
                    await _context.SaveChangesAsync();

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
            var modusOperandi = await _context.ModusOperandi.FindAsync(id);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modusOperandi);
                    await _context.SaveChangesAsync();

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
            var modusOperandi = await _context.ModusOperandi.FindAsync(id);
            if (modusOperandi == null)
            {
                return NotFound();
            }

            // Cambiamos el estado (si estaba activo, lo desactivamos y viceversa)
            modusOperandi.Activo = !modusOperandi.Activo;
            await _context.SaveChangesAsync();

            string estado = modusOperandi.Activo ? "activado" : "desactivado";
            TempData["Success"] = $"Modus operandi '{modusOperandi.Nombre}' {estado} exitosamente";
            
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

