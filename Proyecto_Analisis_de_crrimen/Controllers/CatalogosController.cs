using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    public class CatalogosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // TIPOS DE CRIMEN
        // ============================================

        // GET: Catalogos/TiposCrimen
        public async Task<IActionResult> TiposCrimen()
        {
            var tipos = await _context.TiposCrimen
                .OrderBy(t => t.Nombre)
                .ToListAsync();
            return View(tipos);
        }

        // GET: Catalogos/CrearTipoCrimen
        public IActionResult CrearTipoCrimen()
        {
            return View();
        }

        // POST: Catalogos/CrearTipoCrimen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTipoCrimen(TipoCrimen tipoCrimen)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tipoCrimen.FechaCreacion = DateTime.Now;
                    tipoCrimen.Activo = true;

                    _context.TiposCrimen.Add(tipoCrimen);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Tipo de crimen '{tipoCrimen.Nombre}' creado exitosamente";
                    return RedirectToAction(nameof(TiposCrimen));
                }
                catch (DbUpdateException dbEx)
                {
                    if (dbEx.InnerException?.Message.Contains("UNIQUE") == true)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un tipo de crimen con este nombre");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al guardar: " + dbEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(tipoCrimen);
        }

        // GET: Catalogos/EditarTipoCrimen/5
        public async Task<IActionResult> EditarTipoCrimen(int id)
        {
            var tipoCrimen = await _context.TiposCrimen.FindAsync(id);
            if (tipoCrimen == null)
            {
                return NotFound();
            }
            return View(tipoCrimen);
        }

        // POST: Catalogos/EditarTipoCrimen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTipoCrimen(int id, TipoCrimen tipoCrimen)
        {
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
                    if (dbEx.InnerException?.Message.Contains("UNIQUE") == true)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un tipo de crimen con este nombre");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar: " + dbEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(tipoCrimen);
        }

        // POST: Catalogos/DesactivarTipoCrimen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarTipoCrimen(int id)
        {
            var tipoCrimen = await _context.TiposCrimen.FindAsync(id);
            if (tipoCrimen == null)
            {
                return NotFound();
            }

            tipoCrimen.Activo = !tipoCrimen.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tipo de crimen '{tipoCrimen.Nombre}' {(tipoCrimen.Activo ? "activado" : "desactivado")} exitosamente";
            return RedirectToAction(nameof(TiposCrimen));
        }

        // ============================================
        // MODUS OPERANDI
        // ============================================

        // GET: Catalogos/ModusOperandi
        public async Task<IActionResult> ModusOperandi()
        {
            var modus = await _context.ModusOperandi
                .OrderBy(m => m.Nombre)
                .ToListAsync();
            return View(modus);
        }

        // GET: Catalogos/CrearModusOperandi
        public IActionResult CrearModusOperandi()
        {
            return View();
        }

        // POST: Catalogos/CrearModusOperandi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearModusOperandi(ModusOperandi modusOperandi)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    modusOperandi.FechaCreacion = DateTime.Now;
                    modusOperandi.Activo = true;

                    _context.ModusOperandi.Add(modusOperandi);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Modus operandi '{modusOperandi.Nombre}' creado exitosamente";
                    return RedirectToAction(nameof(ModusOperandi));
                }
                catch (DbUpdateException dbEx)
                {
                    if (dbEx.InnerException?.Message.Contains("UNIQUE") == true)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un modus operandi con este nombre");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al guardar: " + dbEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(modusOperandi);
        }

        // GET: Catalogos/EditarModusOperandi/5
        public async Task<IActionResult> EditarModusOperandi(int id)
        {
            var modusOperandi = await _context.ModusOperandi.FindAsync(id);
            if (modusOperandi == null)
            {
                return NotFound();
            }
            return View(modusOperandi);
        }

        // POST: Catalogos/EditarModusOperandi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarModusOperandi(int id, ModusOperandi modusOperandi)
        {
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
                    if (dbEx.InnerException?.Message.Contains("UNIQUE") == true)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un modus operandi con este nombre");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar: " + dbEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(modusOperandi);
        }

        // POST: Catalogos/DesactivarModusOperandi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarModusOperandi(int id)
        {
            var modusOperandi = await _context.ModusOperandi.FindAsync(id);
            if (modusOperandi == null)
            {
                return NotFound();
            }

            modusOperandi.Activo = !modusOperandi.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Modus operandi '{modusOperandi.Nombre}' {(modusOperandi.Activo ? "activado" : "desactivado")} exitosamente";
            return RedirectToAction(nameof(ModusOperandi));
        }
    }
}

