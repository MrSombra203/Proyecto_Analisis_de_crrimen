using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Attributes;

namespace Proyecto_Analisis_de_crimen.Controllers
{
    [RequireAdmin] // Solo administradores pueden ver diagnósticos
    public class DiagnosticoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Diagnostico/VerificarUsuarios
        public async Task<IActionResult> VerificarUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();

            var diagnosticos = new List<object>();

            foreach (var usuario in usuarios)
            {
                // Verificar contraseñas conocidas (texto plano según la BD)
                var coincideAdmin123 = usuario.Password == "Admin123!";
                var coincideUser123 = usuario.Password == "User123!";

                diagnosticos.Add(new
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Activo = usuario.Activo,
                    PasswordEnBD = usuario.Password?.Substring(0, Math.Min(10, usuario.Password?.Length ?? 0)) + "..." ?? "N/A",
                    CoincideAdmin123 = coincideAdmin123,
                    CoincideUser123 = coincideUser123,
                    Rol = usuario.Rol?.Nombre
                });
            }

            ViewBag.Diagnosticos = diagnosticos;
            return View();
        }
    }
}




l