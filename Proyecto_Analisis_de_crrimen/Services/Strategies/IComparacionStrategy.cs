using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services.Strategies
{
    /// <summary>
    /// Interfaz para estrategias de comparación de escenas (Strategy Pattern)
    /// Aplica OCP (Open/Closed Principle): Extensible sin modificar código existente
    /// Permite agregar nuevos algoritmos de comparación sin cambiar el código cliente
    /// </summary>
    public interface IComparacionStrategy
    {
        /// <summary>
        /// Nombre descriptivo de la estrategia
        /// </summary>
        string Nombre { get; }

        /// <summary>
        /// Compara dos escenas y calcula un porcentaje de similitud (0-100%)
        /// </summary>
        ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada);
    }
}

