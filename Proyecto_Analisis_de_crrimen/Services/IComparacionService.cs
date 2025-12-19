using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Interfaz para el servicio de comparación (DIP - Dependency Inversion Principle)
    /// Permite diferentes implementaciones del algoritmo de comparación
    /// </summary>
    public interface IComparacionService
    {
        ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada);
        List<ComparacionResultado> BuscarEscenasSimilares(EscenaCrimen escenaBase, List<EscenaCrimen> todasLasEscenas, double umbralMinimo = 60.0);
        List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas);
    }
}



