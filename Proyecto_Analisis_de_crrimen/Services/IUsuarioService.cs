using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios (SRP)
    /// </summary>
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario> CrearUsuarioAsync(Usuario usuario);
        Task<Usuario> ActualizarUsuarioAsync(int id, Usuario usuario, string? nuevaPassword);
        Task<bool> CambiarEstadoUsuarioAsync(int id);
        Task<bool> ValidarUsuarioAsync(Usuario usuario, int? excludeId = null);
        Task<IEnumerable<Rol>> ObtenerRolesAsync();
    }
}



