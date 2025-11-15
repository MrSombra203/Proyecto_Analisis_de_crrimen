using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthenticationService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(AuthenticationService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nombreUsuario, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "El nombre de usuario y la contraseña son obligatorios");
                return View();
            }

            // Usar el servicio de autenticación (ya verifica usuario activo)
            var usuario = await _authService.AuthenticateAsync(nombreUsuario, password);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Nombre de usuario o contraseña incorrectos, o cuenta inactiva");
                return View();
            }

            // Guardar información en sesión
            HttpContext.Session.SetInt32("UserId", usuario.Id);
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetInt32("RolId", usuario.RolId);
            HttpContext.Session.SetString("RolNombre", usuario.Rol?.Nombre ?? "");

            TempData["Success"] = $"Bienvenido, {usuario.NombreCompleto}";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Redirigir según el rol del usuario
            // Solo administradores pueden acceder al dashboard
            if (usuario.RolId == 1) // 1 = Administrador
            {
                return RedirectToAction("Dashboard", "EscenaCrimen");
            }
            else
            {
                // Usuarios no administradores son redirigidos al índice de escenas
                return RedirectToAction("Index", "EscenaCrimen");
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login", "Auth");
        }

        // GET: Auth/Logout (para enlaces directos)
        public IActionResult LogoutGet()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login", "Auth");
        }

        // GET: Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
