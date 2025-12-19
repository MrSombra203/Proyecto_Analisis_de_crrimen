using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Repositories;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio de autenticación: valida credenciales, verifica usuarios/emails y actualiza acceso.
    /// NOTA: Almacena contraseñas en texto plano. En producción usar hashing (bcrypt, PBKDF2).
    /// Aplica SRP: Responsabilidad única de autenticación
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public AuthenticationService(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context; // Necesario para Include con Rol
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
            await _unitOfWork.SaveChangesAsync();

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
            if (excludeId.HasValue)
            {
                return await _unitOfWork.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != excludeId.Value);
            }
            return await _unitOfWork.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
        }

        /// <summary>
        /// Verifica si un email ya existe. excludeId permite excluir un usuario (útil al editar).
        /// </summary>
        public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.Usuarios.AnyAsync(u => u.Email == email && u.Id != excludeId.Value);
            }
            return await _unitOfWork.Usuarios.AnyAsync(u => u.Email == email);
        }
    }
}
