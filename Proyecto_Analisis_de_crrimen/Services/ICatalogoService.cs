using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de catálogos (SRP)
    /// </summary>
    public interface ICatalogoService
    {
        // Tipos de Crimen
        Task<IEnumerable<TipoCrimen>> ObtenerTiposCrimenAsync();
        Task<TipoCrimen?> ObtenerTipoCrimenPorIdAsync(int id);
        Task<TipoCrimen> CrearTipoCrimenAsync(TipoCrimen tipoCrimen);
        Task<TipoCrimen> ActualizarTipoCrimenAsync(TipoCrimen tipoCrimen);
        Task<bool> CambiarEstadoTipoCrimenAsync(int id);
        Task<bool> TipoCrimenExisteAsync(string nombre, int? excludeId = null);

        // Modus Operandi
        Task<IEnumerable<ModusOperandi>> ObtenerModusOperandiAsync();
        Task<ModusOperandi?> ObtenerModusOperandiPorIdAsync(int id);
        Task<ModusOperandi> CrearModusOperandiAsync(ModusOperandi modusOperandi);
        Task<ModusOperandi> ActualizarModusOperandiAsync(ModusOperandi modusOperandi);
        Task<bool> CambiarEstadoModusOperandiAsync(int id);
        Task<bool> ModusOperandiExisteAsync(string nombre, int? excludeId = null);
    }
}



