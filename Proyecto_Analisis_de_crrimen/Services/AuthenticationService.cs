using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Autentica un usuario por nombre de usuario y contraseña
        /// La base de datos almacena contraseñas en texto plano según el script SQL
        /// </summary>
        public async Task<Usuario?> AuthenticateAsync(string nombreUsuario, string password)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
                return null;

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

            if (usuario == null)
                return null;

            // Comparar contraseña directamente (texto plano según la BD)
            if (usuario.Password != password)
                return null;

            // Actualizar último acceso
            usuario.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            return usuario;
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Verifica si un nombre de usuario ya existe
        /// </summary>
        public async Task<bool> UsuarioExisteAsync(string nombreUsuario, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.NombreUsuario == nombreUsuario);
            
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Verifica si un email ya existe
        /// </summary>
        public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.Email == email);
            
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
