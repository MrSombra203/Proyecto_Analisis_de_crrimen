using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación (DIP - Dependency Inversion Principle)
    /// </summary>
    public interface IAuthenticationService
    {
        Task<Usuario?> AuthenticateAsync(string nombreUsuario, string password);
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task<bool> UsuarioExisteAsync(string nombreUsuario, int? excludeId = null);
        Task<bool> EmailExisteAsync(string email, int? excludeId = null);
    }
}



