using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Repositories
{
    /// <summary>
    /// Interfaz específica para EscenaCrimen (ISP - Interface Segregation Principle)
    /// Define operaciones específicas del dominio de escenas de crimen
    /// </summary>
    public interface IEscenaCrimenRepository : IRepository<EscenaCrimen>
    {
        Task<EscenaCrimen?> GetByIdWithRelationsAsync(int id);
        Task<IEnumerable<EscenaCrimen>> GetAllWithRelationsAsync();
        Task<IEnumerable<EscenaCrimen>> GetByTipoCrimenAsync(int tipoCrimenId);
        Task<IEnumerable<EscenaCrimen>> GetByAreaGeograficaAsync(AreaGeografica area);
        Task<IEnumerable<EscenaCrimen>> GetByModusOperandiAsync(int modusOperandiId);
        Task<IEnumerable<EscenaCrimen>> GetUltimasEscenasAsync(int cantidad);
    }
}



