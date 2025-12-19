using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de escenas de crimen (SRP - Single Responsibility)
    /// Separa la lógica de negocio de los controladores
    /// </summary>
    public interface IEscenaCrimenService
    {
        Task<IEnumerable<EscenaCrimen>> ObtenerTodasAsync();
        Task<EscenaCrimen?> ObtenerPorIdAsync(int id);
        Task<EscenaCrimen> RegistrarEscenaAsync(EscenaCrimen escena, string usuarioRegistro);
        Task<bool> ValidarEscenaAsync(EscenaCrimen escena);
        Task<IEnumerable<TipoCrimen>> ObtenerTiposCrimenActivosAsync();
        Task<IEnumerable<ModusOperandi>> ObtenerModusOperandiActivosAsync();
        Task<int> ObtenerTotalEscenasAsync();
        Task<IEnumerable<EscenaCrimen>> ObtenerUltimasEscenasAsync(int cantidad);
    }
}



