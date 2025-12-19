using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Repositories;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio de gestión de usuarios (SRP)
    /// Maneja la lógica de negocio para usuarios
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        private readonly ApplicationDbContext _context;
        private const int LONGITUD_MINIMA_PASSWORD = 6;
        private const string PATRON_EMAIL = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        public UsuarioService(IUnitOfWork unitOfWork, IAuthenticationService authenticationService, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _context = context; // Necesario para Include con Rol
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario)
        {
            usuario.FechaCreacion = DateTime.Now;
            usuario.Activo = true;
            usuario.NombreUsuario = usuario.NombreUsuario?.Trim() ?? "";
            usuario.Email = usuario.Email?.Trim() ?? "";
            usuario.NombreCompleto = usuario.NombreCompleto?.Trim() ?? "";

            await _unitOfWork.Usuarios.AddAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> ActualizarUsuarioAsync(int id, Usuario usuario, string? nuevaPassword)
        {
            var usuarioExistente = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuarioExistente == null)
                throw new ArgumentException("Usuario no encontrado", nameof(id));

            usuarioExistente.NombreUsuario = usuario.NombreUsuario?.Trim() ?? "";
            usuarioExistente.Email = usuario.Email?.Trim() ?? "";
            usuarioExistente.NombreCompleto = usuario.NombreCompleto?.Trim() ?? "";
            usuarioExistente.RolId = usuario.RolId;
            usuarioExistente.Activo = usuario.Activo;

            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                usuarioExistente.Password = nuevaPassword;
            }

            _unitOfWork.Usuarios.Update(usuarioExistente);
            await _unitOfWork.SaveChangesAsync();
            return usuarioExistente;
        }

        public async Task<bool> CambiarEstadoUsuarioAsync(int id)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
                return false;

            usuario.Activo = !usuario.Activo;
            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidarUsuarioAsync(Usuario usuario, int? excludeId = null)
        {
            // Validar nombre de usuario único
            if (await _authenticationService.UsuarioExisteAsync(usuario.NombreUsuario, excludeId))
                return false;

            // Validar email único y formato
            if (await _authenticationService.EmailExisteAsync(usuario.Email, excludeId))
                return false;

            if (!string.IsNullOrWhiteSpace(usuario.Email))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(
                    PATRON_EMAIL,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!emailRegex.IsMatch(usuario.Email.Trim()))
                    return false;
            }

            // Validar contraseña (solo al crear)
            if (!excludeId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(usuario.Password) || usuario.Password.Length < LONGITUD_MINIMA_PASSWORD)
                    return false;
            }

            // Validar rol existe
            if (usuario.RolId > 0)
            {
                var rolExiste = await _unitOfWork.Roles.AnyAsync(r => r.Id == usuario.RolId);
                if (!rolExiste)
                    return false;
            }

            return true;
        }

        public async Task<IEnumerable<Rol>> ObtenerRolesAsync()
        {
            return await _unitOfWork.Roles.GetAllAsync();
        }
    }
}

