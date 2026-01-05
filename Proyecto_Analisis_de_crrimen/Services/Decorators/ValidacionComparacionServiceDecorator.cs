using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services.Decorators
{
    /// <summary>
    /// Decorador que agrega validaciones adicionales a las comparaciones
    /// Aplica Decorator Pattern: Agrega validación sin modificar la clase base
    /// Aplica SRP: Responsabilidad única de validación
    /// </summary>
    public class ValidacionComparacionServiceDecorator : IComparacionServiceDecorator
    {
        public IComparacionService ServicioBase { get; }

        public ValidacionComparacionServiceDecorator(IComparacionService servicioBase)
        {
            ServicioBase = servicioBase ?? throw new ArgumentNullException(nameof(servicioBase));
        }

        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            // Validaciones adicionales antes de comparar
            ValidarEscena(escenaBase, nameof(escenaBase));
            ValidarEscena(escenaComparada, nameof(escenaComparada));

            // Validar que las escenas tengan datos mínimos requeridos
            if (escenaBase.TipoCrimenId <= 0)
                throw new ArgumentException("La escena base debe tener un tipo de crimen válido", nameof(escenaBase));

            if (escenaComparada.TipoCrimenId <= 0)
                throw new ArgumentException("La escena comparada debe tener un tipo de crimen válido", nameof(escenaComparada));

            return ServicioBase.CompararEscenas(escenaBase, escenaComparada);
        }

        public List<ComparacionResultado> BuscarEscenasSimilares(
            EscenaCrimen escenaBase,
            List<EscenaCrimen> todasLasEscenas,
            double umbralMinimo = 60.0)
        {
            ValidarEscena(escenaBase, nameof(escenaBase));

            if (todasLasEscenas == null || todasLasEscenas.Count == 0)
                throw new ArgumentException("La lista de escenas no puede estar vacía", nameof(todasLasEscenas));

            if (umbralMinimo < 0 || umbralMinimo > 100)
                throw new ArgumentException("El umbral mínimo debe estar entre 0 y 100", nameof(umbralMinimo));

            return ServicioBase.BuscarEscenasSimilares(escenaBase, todasLasEscenas, umbralMinimo);
        }

        public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
        {
            if (todasLasEscenas == null)
                throw new ArgumentNullException(nameof(todasLasEscenas));

            if (todasLasEscenas.Count < 3)
                return new List<List<EscenaCrimen>>(); // No hay suficientes escenas para formar una serie

            return ServicioBase.DetectarCrimenesEnSerie(todasLasEscenas);
        }

        private void ValidarEscena(EscenaCrimen escena, string paramName)
        {
            if (escena == null)
                throw new ArgumentNullException(paramName);

            if (string.IsNullOrWhiteSpace(escena.Ubicacion))
                throw new ArgumentException("La ubicación de la escena es obligatoria", paramName);
        }
    }
}

