using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    // Controlador para el login y logout de usuarios
    // Se encarga de autenticar usuarios y manejar sus sesiones
    public class AuthController : Controller
    {
        // ID del rol de administrador (usado para redirecciones)
        private const int ROL_ADMINISTRADOR = 1;

        private readonly AuthenticationService _authService;
        private readonly ApplicationDbContext _context;

        // El constructor recibe los servicios que necesita mediante inyección de dependencias
        public AuthController(AuthenticationService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // Muestra la página de login
        // Si el usuario venía de otra página, guardamos la URL para redirigirlo después
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Procesa el formulario cuando el usuario intenta iniciar sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nombreUsuario, string password, string? returnUrl = null)
        {
            // Primero verificamos que haya ingresado algo
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "El nombre de usuario y la contraseña son obligatorios");
                return View();
            }

            // Intentamos autenticar al usuario
            var usuario = await _authService.AuthenticateAsync(nombreUsuario, password);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Nombre de usuario o contraseña incorrectos, o cuenta inactiva");
                return View();
            }

            // Si todo está bien, guardamos sus datos en la sesión
            GuardarUsuarioEnSesion(usuario);

            TempData["Success"] = $"Bienvenido, {usuario.NombreCompleto}";

            // Si venía de otra página, lo regresamos ahí
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Si no, lo mandamos a su página según su rol
            return RedirigirSegunRol(usuario.RolId);
        }

        // Cierra la sesión cuando el usuario hace clic en "Cerrar sesión" desde un formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return CerrarSesion();
        }

        // Versión GET del logout, para cuando se hace clic directo en un enlace
        public IActionResult LogoutGet()
        {
            return CerrarSesion();
        }

        // Muestra la página cuando un usuario intenta acceder a algo sin permisos
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ============================================
        // Métodos auxiliares
        // ============================================

        // Guarda los datos del usuario en la sesión para que estén disponibles en todas las peticiones
        // Esto es necesario porque HTTP no guarda estado entre peticiones
        private void GuardarUsuarioEnSesion(Usuario usuario)
        {
            HttpContext.Session.SetInt32("UserId", usuario.Id);
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetInt32("RolId", usuario.RolId);
            HttpContext.Session.SetString("RolNombre", usuario.Rol?.Nombre ?? "");
        }

        // Decide a dónde mandar al usuario después del login según su rol
        // Los administradores van al dashboard, los demás a la lista de escenas
        private IActionResult RedirigirSegunRol(int rolId)
        {
            if (rolId == ROL_ADMINISTRADOR)
            {
                return RedirectToAction("Dashboard", "EscenaCrimen");
            }
            else
            {
                return RedirectToAction("Index", "EscenaCrimen");
            }
        }

        // Método común para cerrar sesión, usado tanto por Logout como LogoutGet
        // Limpia la sesión y redirige al login
        private IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login", "Auth");
        }
    }
}
