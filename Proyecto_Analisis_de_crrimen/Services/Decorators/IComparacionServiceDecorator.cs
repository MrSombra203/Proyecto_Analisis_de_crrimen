using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services.Decorators
{
    /// <summary>
    /// Interfaz base para decoradores de IComparacionService (Decorator Pattern)
    /// Permite agregar funcionalidades adicionales sin modificar la implementación base
    /// Aplica OCP: Extensible sin modificar código existente
    /// </summary>
    public interface IComparacionServiceDecorator : IComparacionService
    {
        /// <summary>
        /// Servicio base que se está decorando
        /// </summary>
        IComparacionService ServicioBase { get; }
    }
}

