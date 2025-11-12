using System.Collections.Generic;

namespace Proyecto_Analisis_de_crimen.Models
{
    public class ComparacionResultado
    {
        public EscenaCrimen EscenaBase { get; set; }
        public EscenaCrimen EscenaComparada { get; set; }
        public double PorcentajeSimilitud { get; set; }
        public List<string> Coincidencias { get; set; }
        public ClasificacionCrimen Clasificacion { get; set; }

        public ComparacionResultado()
        {
            Coincidencias = new List<string>();
        }
    }

    public enum ClasificacionCrimen
    {
        CrimenEnSerie,
        ConexionProbable,
        SimilitudBaja
    }
}