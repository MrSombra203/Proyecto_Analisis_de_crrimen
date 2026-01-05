using Proyecto_Analisis_de_crimen.Services.Strategies;

namespace Proyecto_Analisis_de_crimen.Services.Factories
{
    /// <summary>
    /// Factory para crear estrategias de comparación (Factory Pattern)
    /// Aplica DIP: Depende de abstracciones (IComparacionStrategy)
    /// Permite crear diferentes estrategias sin conocer sus implementaciones concretas
    /// </summary>
    public interface IComparacionStrategyFactory
    {
        /// <summary>
        /// Crea una estrategia de comparación basada en el tipo especificado
        /// </summary>
        IComparacionStrategy CrearEstrategia(TipoComparacionStrategy tipo);

        /// <summary>
        /// Obtiene la estrategia por defecto del sistema
        /// </summary>
        IComparacionStrategy ObtenerEstrategiaPorDefecto();

        /// <summary>
        /// Obtiene todas las estrategias disponibles
        /// </summary>
        IEnumerable<IComparacionStrategy> ObtenerTodasLasEstrategias();
    }

    /// <summary>
    /// Tipos de estrategias de comparación disponibles
    /// </summary>
    public enum TipoComparacionStrategy
    {
        Estandar,
        Geografica
    }
}

