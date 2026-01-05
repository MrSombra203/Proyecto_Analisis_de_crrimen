using Proyecto_Analisis_de_crimen.Models;
using Microsoft.Extensions.Logging;

namespace Proyecto_Analisis_de_crimen.Services.Decorators
{
    /// <summary>
    /// Decorador que agrega funcionalidad de logging a las comparaciones
    /// Aplica Decorator Pattern: Agrega comportamiento sin modificar la clase base
    /// Aplica SRP: Responsabilidad única de logging
    /// </summary>
    public class LoggingComparacionServiceDecorator : IComparacionServiceDecorator
    {
        public IComparacionService ServicioBase { get; }
        private readonly ILogger<LoggingComparacionServiceDecorator> _logger;

        public LoggingComparacionServiceDecorator(
            IComparacionService servicioBase,
            ILogger<LoggingComparacionServiceDecorator> logger)
        {
            ServicioBase = servicioBase ?? throw new ArgumentNullException(nameof(servicioBase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            _logger.LogInformation(
                "Iniciando comparación entre escenas: Base={EscenaBaseId}, Comparada={EscenaComparadaId}",
                escenaBase?.Id ?? 0,
                escenaComparada?.Id ?? 0);

            try
            {
                var resultado = ServicioBase.CompararEscenas(escenaBase, escenaComparada);

                _logger.LogInformation(
                    "Comparación completada: Similitud={Porcentaje}%, Clasificación={Clasificacion}",
                    resultado.PorcentajeSimilitud,
                    resultado.Clasificacion);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al comparar escenas: Base={EscenaBaseId}, Comparada={EscenaComparadaId}",
                    escenaBase?.Id ?? 0,
                    escenaComparada?.Id ?? 0);
                throw;
            }
        }

        public List<ComparacionResultado> BuscarEscenasSimilares(
            EscenaCrimen escenaBase,
            List<EscenaCrimen> todasLasEscenas,
            double umbralMinimo = 60.0)
        {
            _logger.LogInformation(
                "Buscando escenas similares: EscenaBase={EscenaBaseId}, TotalEscenas={Total}, Umbral={Umbral}",
                escenaBase?.Id ?? 0,
                todasLasEscenas?.Count ?? 0,
                umbralMinimo);

            var resultados = ServicioBase.BuscarEscenasSimilares(escenaBase, todasLasEscenas, umbralMinimo);

            _logger.LogInformation(
                "Búsqueda completada: EscenasEncontradas={Cantidad}",
                resultados.Count);

            return resultados;
        }

        public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
        {
            _logger.LogInformation(
                "Detectando crímenes en serie: TotalEscenas={Total}",
                todasLasEscenas?.Count ?? 0);

            var series = ServicioBase.DetectarCrimenesEnSerie(todasLasEscenas);

            _logger.LogInformation(
                "Detección completada: SeriesEncontradas={Cantidad}",
                series.Count);

            return series;
        }
    }
}

