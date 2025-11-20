using System.Collections.Generic;

namespace Proyecto_Analisis_de_crimen.Models
{
    // Resultado de comparar dos escenas de crimen
    // Contiene toda la información que se muestra al usuario sobre qué tan similares son
    // y por qué se considera que están relacionadas
    public class ComparacionResultado
    {
        // La primera escena que se seleccionó para comparar
        public EscenaCrimen EscenaBase { get; set; }

        // La segunda escena con la que se comparó
        public EscenaCrimen EscenaComparada { get; set; }

        // Qué tan similares son, de 0 a 100
        // Se calcula evaluando varios criterios: tipo de crimen, área, modus operandi,
        // horario, evidencias (con el coeficiente de Jaccard), y características especiales
        public double PorcentajeSimilitud { get; set; }

        // Lista de cosas que tienen en común
        // Ejemplos: "Mismo tipo de crimen", "Misma área geográfica", "Evidencias similares"
        public List<string> Coincidencias { get; set; }

        // Qué tan probable es que estén relacionados
        // - CrimenEnSerie: muy similares, probablemente mismo perpetrador
        // - ConexionProbable: algo similares, podría haber conexión
        // - SimilitudBaja: poco similares, probablemente no relacionados
        public ClasificacionCrimen Clasificacion { get; set; }

        // Constructor: inicializa la lista de coincidencias
        public ComparacionResultado()
        {
            Coincidencias = new List<string>();
        }
    }

    // Clasifica qué tan probable es que dos crímenes estén relacionados
    // Ayuda a los investigadores a saber en qué casos vale la pena investigar más
    public enum ClasificacionCrimen
    {
        // Muy similares, probablemente crímenes en serie del mismo perpetrador
        CrimenEnSerie,

        // Algo similares, podría haber conexión pero hay que investigar más
        ConexionProbable,

        // Poco similares, probablemente no están relacionados
        SimilitudBaja
    }
}