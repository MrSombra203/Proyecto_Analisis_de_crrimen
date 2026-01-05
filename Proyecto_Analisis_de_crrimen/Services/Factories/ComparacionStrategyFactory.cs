using Proyecto_Analisis_de_crimen.Services.Strategies;

namespace Proyecto_Analisis_de_crimen.Services.Factories
{
    /// <summary>
    /// Implementación del Factory para crear estrategias de comparación
    /// Aplica Factory Pattern: Centraliza la creación de objetos complejos
    /// Aplica SRP: Responsabilidad única de crear estrategias
    /// </summary>
    public class ComparacionStrategyFactory : IComparacionStrategyFactory
    {
        private readonly Dictionary<TipoComparacionStrategy, IComparacionStrategy> _estrategias;

        public ComparacionStrategyFactory()
        {
            // Inicializar estrategias disponibles (Singleton por instancia)
            _estrategias = new Dictionary<TipoComparacionStrategy, IComparacionStrategy>
            {
                { TipoComparacionStrategy.Estandar, new ComparacionEstandarStrategy() },
                { TipoComparacionStrategy.Geografica, new ComparacionGeograficaStrategy() }
            };
        }

        public IComparacionStrategy CrearEstrategia(TipoComparacionStrategy tipo)
        {
            if (_estrategias.TryGetValue(tipo, out var estrategia))
            {
                return estrategia;
            }

            // Si no se encuentra, retornar la estrategia por defecto
            return ObtenerEstrategiaPorDefecto();
        }

        public IComparacionStrategy ObtenerEstrategiaPorDefecto()
        {
            return _estrategias[TipoComparacionStrategy.Estandar];
        }

        public IEnumerable<IComparacionStrategy> ObtenerTodasLasEstrategias()
        {
            return _estrategias.Values;
        }
    }
}

