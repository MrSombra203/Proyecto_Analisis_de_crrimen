using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;
using Proyecto_Analisis_de_crimen.Attributes;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    // Controlador para gestionar usuarios del sistema
    // Solo los administradores pueden acceder aquí
    [RequireAdmin]
    public class UsuariosController : Controller
    {
        // Constantes que usamos en las validaciones
        private const int LONGITUD_MINIMA_PASSWORD = 6;
        private const string PATRON_EMAIL = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        private readonly ApplicationDbContext _context;
        private readonly AuthenticationService _authService;

        // Constructor con inyección de dependencias
        public UsuariosController(ApplicationDbContext context, AuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Muestra la lista de todos los usuarios, ordenados por nombre
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)  // Traemos también el rol de cada usuario
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();
            
            return View(usuarios);
        }

        // Muestra el formulario para crear un nuevo usuario
        public async Task<IActionResult> Crear()
        {
            await CargarRolesEnViewBag();
            return View();
        }

        // Procesa el formulario cuando se crea un nuevo usuario
        // Validamos que el nombre de usuario y email sean únicos, y que la contraseña tenga al menos 6 caracteres
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario)
        {
            // Validamos todo antes de guardar
            await ValidarUsuario(usuario);

            if (ModelState.IsValid)
            {
                try
                {
                    // Creamos una nueva instancia limpia para evitar problemas con las propiedades de navegación
                    var nuevoUsuario = CrearNuevoUsuario(usuario);

                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Usuario '{nuevoUsuario.NombreUsuario}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    ManejarErrorBaseDatos(dbEx, "guardar");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el usuario: " + ex.Message);
                }
            }

            await CargarRolesEnViewBag();
            return View(usuario);
        }

        // Muestra el formulario de edición con los datos del usuario
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Por seguridad, no mostramos la contraseña actual
            usuario.Password = "";

            await CargarRolesEnViewBag(usuario.RolId);
            return View(usuario);
        }

        // Procesa el formulario cuando se edita un usuario
        // La contraseña solo se actualiza si se proporciona una nueva, si no, se mantiene la anterior
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Usuario usuario)
        {
            // Verificamos que el ID coincida
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Validamos, pero excluimos el usuario actual de las validaciones de unicidad
            await ValidarUsuario(usuario, id);

            // Validar y procesar contraseña
            string? nuevaPassword = ObtenerPasswordParaActualizar(usuario);
            if (!string.IsNullOrWhiteSpace(nuevaPassword) && nuevaPassword.Length < LONGITUD_MINIMA_PASSWORD)
            {
                ModelState.AddModelError("NuevaPassword", $"La contraseña debe tener al menos {LONGITUD_MINIMA_PASSWORD} caracteres");
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

                    // Actualizar propiedades del usuario
                    ActualizarPropiedadesUsuario(usuarioExistente, usuario, nuevaPassword);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Usuario '{usuario.NombreUsuario}' actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    ManejarErrorBaseDatos(dbEx, "actualizar");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el usuario: " + ex.Message);
                }
            }

            await CargarRolesEnViewBag(usuario.RolId);
            return View(usuario);
        }

        // Activa o desactiva un usuario (cambia su estado)
        // Un usuario desactivado no puede iniciar sesión
        // No permitimos que un usuario se desactive a sí mismo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // No permitimos que se desactive a sí mismo
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "No puede desactivar su propia cuenta";
                return RedirectToAction(nameof(Index));
            }

            // Cambiamos el estado (si estaba activo, lo desactivamos y viceversa)
            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();

            string estado = usuario.Activo ? "activado" : "desactivado";
            TempData["Success"] = $"Usuario '{usuario.NombreUsuario}' {estado} exitosamente";
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // Métodos auxiliares
        // ============================================

        // Carga los roles disponibles en el ViewBag para mostrarlos en los dropdowns
        // Si se pasa un rolSeleccionado, ese será el que aparezca seleccionado (útil al editar)
        private async Task CargarRolesEnViewBag(int? rolSeleccionado = null)
        {
            ViewBag.Roles = new SelectList(
                await _context.Roles.OrderBy(r => r.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                rolSeleccionado
            );
        }

        // Valida todos los datos del usuario antes de guardarlo
        // Verifica que el nombre de usuario y email sean únicos, que la contraseña tenga al menos 6 caracteres, y que el rol exista
        // Si se pasa excludeId, excluimos ese usuario de las validaciones de unicidad (útil al editar)
        private async Task ValidarUsuario(Usuario usuario, int? excludeId = null)
        {
            // Verificamos que el nombre de usuario no esté en uso
            if (await _authService.UsuarioExisteAsync(usuario.NombreUsuario, excludeId))
            {
                ModelState.AddModelError("NombreUsuario", "Este nombre de usuario ya está en uso");
            }

            // Validamos el email
            ValidarEmail(usuario.Email, excludeId);

            // La contraseña solo se valida al crear, no al editar (a menos que se esté cambiando)
            if (!excludeId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(usuario.Password) || usuario.Password.Length < LONGITUD_MINIMA_PASSWORD)
                {
                    ModelState.AddModelError("Password", $"La contraseña debe tener al menos {LONGITUD_MINIMA_PASSWORD} caracteres");
                }
            }

            // Verificamos que el rol que seleccionó realmente exista
            if (usuario.RolId > 0)
            {
                var rolExiste = await _context.Roles.AnyAsync(r => r.Id == usuario.RolId);
                if (!rolExiste)
                {
                    ModelState.AddModelError("RolId", "El rol seleccionado no existe");
                }
            }
        }

        // Valida que el email tenga un formato válido y que no esté en uso
        private async Task ValidarEmail(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "El correo electrónico es obligatorio");
                return;
            }

            // Verificamos el formato con una expresión regular
            var emailRegex = new System.Text.RegularExpressions.Regex(
                PATRON_EMAIL,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!emailRegex.IsMatch(email.Trim()))
            {
                ModelState.AddModelError("Email", "El formato del correo electrónico no es válido");
                return;
            }

            // Verificamos que no esté en uso por otro usuario
            if (await _authService.EmailExisteAsync(email, excludeId))
            {
                ModelState.AddModelError("Email", "Este email ya está en uso");
            }
        }

        // Crea un nuevo objeto Usuario limpio con los datos del formulario
        // Lo hacemos así para evitar problemas con las propiedades de navegación que vienen del binding
        private Usuario CrearNuevoUsuario(Usuario usuario)
        {
            return new Usuario
            {
                NombreUsuario = usuario.NombreUsuario?.Trim() ?? "",
                Email = usuario.Email?.Trim() ?? "",
                Password = usuario.Password,  // Nota: las contraseñas se guardan en texto plano según el diseño de la BD
                NombreCompleto = usuario.NombreCompleto?.Trim() ?? "",
                RolId = usuario.RolId,
                Activo = usuario.Activo,
                FechaCreacion = DateTime.Now
            };
        }

        // Determina qué contraseña usar al actualizar
        // Si hay una NuevaPassword, usamos esa; si no, usamos Password; si ambas están vacías, retornamos null (no se cambia)
        private string? ObtenerPasswordParaActualizar(Usuario usuario)
        {
            string? nuevaPassword = usuario.NuevaPassword;
            
            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                nuevaPassword = usuario.Password;
            }

            // Si no hay contraseña nueva, removemos las validaciones de contraseña
            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                ModelState.Remove("Password");
                ModelState.Remove("NuevaPassword");
                return null;
            }

            return nuevaPassword;
        }

        // Actualiza las propiedades del usuario existente con los nuevos valores del formulario
        private void ActualizarPropiedadesUsuario(Usuario usuarioExistente, Usuario usuarioNuevo, string? nuevaPassword)
        {
            usuarioExistente.NombreUsuario = usuarioNuevo.NombreUsuario?.Trim() ?? "";
            usuarioExistente.Email = usuarioNuevo.Email?.Trim() ?? "";
            usuarioExistente.NombreCompleto = usuarioNuevo.NombreCompleto?.Trim() ?? "";
            usuarioExistente.RolId = usuarioNuevo.RolId;
            usuarioExistente.Activo = usuarioNuevo.Activo;

            // Solo actualizamos la contraseña si se proporcionó una nueva
            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                usuarioExistente.Password = nuevaPassword;
            }
        }

        // Maneja los errores de base de datos de forma consistente
        // Muestra el mensaje de error y los detalles si están disponibles
        private void ManejarErrorBaseDatos(DbUpdateException dbEx, string operacion)
        {
            var errorMessage = $"Error al {operacion} en la base de datos: {dbEx.Message}";
            if (dbEx.InnerException != null)
            {
                errorMessage += " | Detalles: " + dbEx.InnerException.Message;
            }
            ModelState.AddModelError("", errorMessage);
        }
    }
}
