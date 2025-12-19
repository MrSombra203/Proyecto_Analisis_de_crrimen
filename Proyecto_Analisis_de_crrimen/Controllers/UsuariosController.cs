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
    // Aplica DIP: Depende de interfaces, no de implementaciones concretas
    [RequireAdmin]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        // Constructor con inyección de dependencias (DIP)
        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // Muestra la lista de todos los usuarios, ordenados por nombre
        public async Task<IActionResult> Index()
        {
            var usuarios = (await _usuarioService.ObtenerTodosAsync())
                .OrderBy(u => u.NombreUsuario);
            
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
            // Validamos todo antes de guardar usando el servicio
            if (!await _usuarioService.ValidarUsuarioAsync(usuario))
            {
                // Agregar errores específicos si es necesario
                ModelState.AddModelError("", "Los datos del usuario no son válidos");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var nuevoUsuario = await _usuarioService.CrearUsuarioAsync(usuario);
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
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);

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

            // Validar usando el servicio
            if (!await _usuarioService.ValidarUsuarioAsync(usuario, id))
            {
                ModelState.AddModelError("", "Los datos del usuario no son válidos");
            }

            // Validar y procesar contraseña
            string? nuevaPassword = ObtenerPasswordParaActualizar(usuario);

            if (ModelState.IsValid)
            {
                try
                {
                    await _usuarioService.ActualizarUsuarioAsync(id, usuario, nuevaPassword);
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
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
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

            var exito = await _usuarioService.CambiarEstadoUsuarioAsync(id);
            if (exito)
            {
                string estado = usuario.Activo ? "desactivado" : "activado";
                TempData["Success"] = $"Usuario '{usuario.NombreUsuario}' {estado} exitosamente";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // Métodos auxiliares
        // ============================================

        // Carga los roles disponibles en el ViewBag para mostrarlos en los dropdowns
        // Si se pasa un rolSeleccionado, ese será el que aparezca seleccionado (útil al editar)
        private async Task CargarRolesEnViewBag(int? rolSeleccionado = null)
        {
            var roles = await _usuarioService.ObtenerRolesAsync();
            ViewBag.Roles = new SelectList(
                roles.OrderBy(r => r.Nombre),
                "Id",
                "Nombre",
                rolSeleccionado
            );
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
