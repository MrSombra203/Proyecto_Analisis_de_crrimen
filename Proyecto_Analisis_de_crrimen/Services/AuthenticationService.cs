using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio de autenticación: valida credenciales, verifica usuarios/emails y actualiza acceso.
    /// NOTA: Almacena contraseñas en texto plano. En producción usar hashing (bcrypt, PBKDF2).
    /// </summary>
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Autentica usuario verificando credenciales. Actualiza fecha de último acceso si es válido.
        /// IMPORTANTE: Compara contraseña en texto plano. En producción usar hashing.
        /// </summary>
        public async Task<Usuario?> AuthenticateAsync(string nombreUsuario, string password)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
                return null;

            // Buscar usuario activo con su rol (eager loading)
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

            if (usuario == null)
                return null;

            // Comparar contraseña (texto plano). En producción: BCrypt.Verify(password, usuario.Password)
            if (usuario.Password != password)
                return null;

            // Actualizar último acceso
            usuario.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            return usuario;
        }

        /// <summary>
        /// Obtiene usuario por ID con su rol incluido.
        /// </summary>
        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Verifica si un nombre de usuario ya existe. excludeId permite excluir un usuario (útil al editar).
        /// </summary>
        public async Task<bool> UsuarioExisteAsync(string nombreUsuario, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.NombreUsuario == nombreUsuario);
            
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// Verifica si un email ya existe. excludeId permite excluir un usuario (útil al editar).
        /// </summary>
        public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.Email == email);
            
            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
