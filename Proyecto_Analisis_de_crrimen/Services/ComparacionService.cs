using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services.Factories;
using Proyecto_Analisis_de_crimen.Services.Strategies;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio CORE del sistema. Implementa el algoritmo de comparación multi-criterio
    /// para calcular similitud entre escenas de crímenes y detectar series criminales.
    /// Aplica SRP: Responsabilidad única de comparación de escenas
    /// Aplica Strategy Pattern: Delega la comparación a estrategias configurables
    /// Aplica DIP: Depende de abstracciones (IComparacionStrategyFactory)
    /// </summary>
    public class ComparacionService : IComparacionService
    {
        private readonly IComparacionStrategyFactory _strategyFactory;
        private readonly IComparacionStrategy _estrategiaPorDefecto;

        // CONFIGURACIÓN DE SERIES
        private const double UMBRAL_SERIE_CRIMINAL = 75.0;
        private const int MINIMO_ESCENAS_PARA_SERIE = 3;

        /// <summary>
        /// Constructor con inyección de dependencias (DIP)
        /// </summary>
        public ComparacionService(IComparacionStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
            _estrategiaPorDefecto = _strategyFactory.ObtenerEstrategiaPorDefecto();
        }

        /// <summary>
        /// Compara dos escenas usando la estrategia por defecto.
        /// Evalúa múltiples criterios con pesos específicos. Retorna clasificación automática.
        /// Aplica Strategy Pattern: Delega la comparación a la estrategia configurada
        /// </summary>
        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            // Validaciones
            if (escenaBase == null)
                throw new ArgumentNullException(nameof(escenaBase));
            
            if (escenaComparada == null)
                throw new ArgumentNullException(nameof(escenaComparada));

            // Usar la estrategia por defecto para realizar la comparación
            // Esto aplica Strategy Pattern: delega la lógica de comparación a la estrategia
            return _estrategiaPorDefecto.CompararEscenas(escenaBase, escenaComparada);
        }

        /// <summary>
        /// Busca escenas similares a una escena base que superen el umbral mínimo.
        /// Retorna resultados ordenados por similitud descendente.
        /// </summary>
        public List<ComparacionResultado> BuscarEscenasSimilares(EscenaCrimen escenaBase, List<EscenaCrimen> todasLasEscenas, double umbralMinimo = 60.0)
        {
            if (escenaBase == null)
                throw new ArgumentNullException(nameof(escenaBase));
            
            if (todasLasEscenas == null)
                throw new ArgumentNullException(nameof(todasLasEscenas));

            if (umbralMinimo < 0 || umbralMinimo > 100)
                throw new ArgumentException("El umbral mínimo debe estar entre 0 y 100", nameof(umbralMinimo));

            var resultados = new List<ComparacionResultado>();
            int escenaBaseId = escenaBase.Id;

            foreach (var escena in todasLasEscenas)
            {
                if (escena.Id == escenaBaseId)
                    continue;

                var comparacion = CompararEscenas(escenaBase, escena);

                if (comparacion.PorcentajeSimilitud >= umbralMinimo)
                {
                    resultados.Add(comparacion);
                }
            }

            return resultados.OrderByDescending(r => r.PorcentajeSimilitud).ToList();
        }

        /// <summary>
        /// Detecta grupos de crímenes que forman series (≥3 crímenes con ≥75% similitud).
        /// Evita duplicados marcando escenas procesadas.
        /// </summary>
        public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
        {
            if (todasLasEscenas == null)
                throw new ArgumentNullException(nameof(todasLasEscenas));

            if (todasLasEscenas.Count < MINIMO_ESCENAS_PARA_SERIE)
                return new List<List<EscenaCrimen>>();

            var series = new List<List<EscenaCrimen>>();
            var procesadas = new HashSet<int>();

            foreach (var escena in todasLasEscenas)
            {
                if (procesadas.Contains(escena.Id))
                    continue;

                var similares = BuscarEscenasSimilares(escena, todasLasEscenas, UMBRAL_SERIE_CRIMINAL);
                int minimoRequerido = MINIMO_ESCENAS_PARA_SERIE - 1;
                
                if (similares.Count >= minimoRequerido)
                {
                    var serie = new List<EscenaCrimen> { escena };
                    serie.AddRange(similares.Select(s => s.EscenaComparada));

                    if (serie.Count >= MINIMO_ESCENAS_PARA_SERIE)
                    {
                        foreach (var e in serie)
                            procesadas.Add(e.Id);

                        series.Add(serie);
                    }
                }
            }

            return series;
        }
    }
}
