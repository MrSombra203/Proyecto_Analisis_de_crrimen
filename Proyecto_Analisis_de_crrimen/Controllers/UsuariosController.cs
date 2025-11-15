using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;
using Proyecto_Analisis_de_crimen.Attributes;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    [RequireAdmin] // Solo administradores pueden gestionar usuarios
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthenticationService _authService;

        public UsuariosController(ApplicationDbContext context, AuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Verificar si el usuario es administrador
        private bool IsAdmin()
        {
            var rolId = HttpContext.Session.GetInt32("RolId");
            return rolId == 1; // 1 = Administrador
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "No tiene permisos para acceder a esta sección";
                return RedirectToAction("AccessDenied", "Auth");
            }

            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Crear
        public async Task<IActionResult> Crear()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "No tiene permisos para crear usuarios";
                return RedirectToAction("AccessDenied", "Auth");
            }

            await CargarRolesEnViewBag();
            return View();
        }

        // POST: Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "No tiene permisos para crear usuarios";
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Validar nombre de usuario único
            if (await _authService.UsuarioExisteAsync(usuario.NombreUsuario))
            {
                ModelState.AddModelError("NombreUsuario", "Este nombre de usuario ya está en uso");
            }

            // Validar email único
            if (await _authService.EmailExisteAsync(usuario.Email))
            {
                ModelState.AddModelError("Email", "Este email ya está en uso");
            }

            // Validar contraseña
            if (string.IsNullOrWhiteSpace(usuario.Password) || usuario.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "La contraseña debe tener al menos 6 caracteres");
            }

            // Validar que el RolId exista
            if (usuario.RolId > 0)
            {
                var rolExiste = await _context.Roles.AnyAsync(r => r.Id == usuario.RolId);
                if (!rolExiste)
                {
                    ModelState.AddModelError("RolId", "El rol seleccionado no existe");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Crear nueva instancia sin propiedades de navegación para evitar problemas
                    var nuevoUsuario = new Usuario
                    {
                        NombreUsuario = usuario.NombreUsuario?.Trim() ?? "",
                        Email = usuario.Email?.Trim() ?? "",
                        Password = usuario.Password, // Texto plano según la BD
                        NombreCompleto = usuario.NombreCompleto?.Trim() ?? "",
                        RolId = usuario.RolId,
                        Activo = usuario.Activo,
                        FechaCreacion = DateTime.Now
                    };

                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Usuario '{nuevoUsuario.NombreUsuario}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    var errorMessage = "Error al guardar en la base de datos: " + dbEx.Message;
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += " | Detalles: " + dbEx.InnerException.Message;
                    }
                    ModelState.AddModelError("", errorMessage);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el usuario: " + ex.Message);
                }
            }

            await CargarRolesEnViewBag();
            return View(usuario);
        }

        // GET: Usuarios/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "No tiene permisos para editar usuarios";
                return RedirectToAction("AccessDenied", "Auth");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            // No mostrar la contraseña actual por seguridad
            usuario.Password = "";

            await CargarRolesEnViewBag(usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Usuario usuario)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "No tiene permisos para editar usuarios";
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Validar nombre de usuario único (excluyendo el actual)
            if (await _authService.UsuarioExisteAsync(usuario.NombreUsuario, id))
            {
                ModelState.AddModelError("NombreUsuario", "Este nombre de usuario ya está en uso");
            }

            // Validar email único (excluyendo el actual)
            if (await _authService.EmailExisteAsync(usuario.Email, id))
            {
                ModelState.AddModelError("Email", "Este email ya está en uso");
            }

            // Validar que el RolId exista
            if (usuario.RolId > 0)
            {
                var rolExiste = await _context.Roles.AnyAsync(r => r.Id == usuario.RolId);
                if (!rolExiste)
                {
                    ModelState.AddModelError("RolId", "El rol seleccionado no existe");
                }
            }

            // Usar NuevaPassword si está disponible, sino usar Password
            string? nuevaPassword = usuario.NuevaPassword;
            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                nuevaPassword = usuario.Password; // Fallback a Password si NuevaPassword está vacío
            }

            // Remover validación de Password si está vacío (no se cambia)
            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                ModelState.Remove("Password");
                ModelState.Remove("NuevaPassword");
            }
            else if (nuevaPassword.Length < 6)
            {
                ModelState.AddModelError("NuevaPassword", "La contraseña debe tener al menos 6 caracteres");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioExistente = await _context.Usuarios.FindAsync(id);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades sin tocar la propiedad de navegación
                    usuarioExistente.NombreUsuario = usuario.NombreUsuario?.Trim() ?? "";
                    usuarioExistente.Email = usuario.Email?.Trim() ?? "";
                    usuarioExistente.NombreCompleto = usuario.NombreCompleto?.Trim() ?? "";
                    usuarioExistente.RolId = usuario.RolId;
                    usuarioExistente.Activo = usuario.Activo;

                    // Actualizar contraseña solo si se proporcionó una nueva
                    if (!string.IsNullOrWhiteSpace(nuevaPassword))
                    {
                        usuarioExistente.Password = nuevaPassword;
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Usuario '{usuario.NombreUsuario}' actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    var errorMessage = "Error al guardar en la base de datos: " + dbEx.Message;
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += " | Detalles: " + dbEx.InnerException.Message;
                    }
                    ModelState.AddModelError("", errorMessage);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el usuario: " + ex.Message);
                }
            }

            await CargarRolesEnViewBag(usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/Desactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "No tiene permisos para esta acción";
                return RedirectToAction("AccessDenied", "Auth");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // No permitir desactivar a sí mismo
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "No puede desactivar su propia cuenta";
                return RedirectToAction(nameof(Index));
            }

            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Usuario '{usuario.NombreUsuario}' {(usuario.Activo ? "activado" : "desactivado")} exitosamente";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRolesEnViewBag(int? rolSeleccionado = null)
        {
            ViewBag.Roles = new SelectList(
                await _context.Roles.OrderBy(r => r.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                rolSeleccionado
            );
        }
    }
}
