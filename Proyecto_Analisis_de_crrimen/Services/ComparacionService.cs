using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio CORE del sistema. Implementa el algoritmo de comparación multi-criterio
    /// para calcular similitud entre escenas de crímenes y detectar series criminales.
    /// </summary>
    public class ComparacionService
    {
        // PESOS DE CRITERIOS (suman 100 puntos)
        private const double PESO_TIPO_CRIMEN = 25.0;        // Tipo de delito (muy importante)
        private const double PESO_MODUS_OPERANDI = 25.0;     // Método operativo (muy importante)
        private const double PESO_AREA_GEOGRAFICA = 20.0;    // Zona geográfica (importante)
        private const double PESO_EVIDENCIAS = 15.0;         // Evidencias físicas (importante)
        private const double PESO_HORARIO = 10.0;            // Horario del crimen (moderado)
        private const double PESO_CARACTERISTICAS = 5.0;     // Características especiales (bajo)
        
        // UMBRALES DE CLASIFICACIÓN
        private const double UMBRAL_CRIMEN_EN_SERIE = 75.0;  // ≥75% = Alta probabilidad
        private const double UMBRAL_CONEXION_PROBABLE = 60.0; // ≥60% = Posible relación
        
        // CONFIGURACIÓN DE SERIES
        private const double UMBRAL_SERIE_CRIMINAL = 75.0;
        private const int MINIMO_ESCENAS_PARA_SERIE = 3;

        /// <summary>
        /// Compara dos escenas y calcula un porcentaje de similitud (0-100%).
        /// Evalúa 6 criterios con pesos específicos. Retorna clasificación automática.
        /// </summary>
        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            // Validaciones
            if (escenaBase == null)
                throw new ArgumentNullException(nameof(escenaBase));
            
            if (escenaComparada == null)
                throw new ArgumentNullException(nameof(escenaComparada));

            // Si es la misma escena, retornar 100% de similitud
            if (escenaBase.Id > 0 && escenaComparada.Id > 0 && escenaBase.Id == escenaComparada.Id)
            {
                return new ComparacionResultado
                {
                    EscenaBase = escenaBase,
                    EscenaComparada = escenaComparada,
                    PorcentajeSimilitud = 100.0,
                    Clasificacion = ClasificacionCrimen.CrimenEnSerie,
                    Coincidencias = new List<string> { "Misma escena de crimen" }
                };
            }

            var resultado = new ComparacionResultado
            {
                EscenaBase = escenaBase,
                EscenaComparada = escenaComparada
            };

            double puntosTotales = 0.0;
            const double puntosMaximos = 100.0;

            // CRITERIO 1: Tipo de Crimen (25 puntos)
            if (escenaBase.TipoCrimenId > 0 && escenaComparada.TipoCrimenId > 0 &&
                escenaBase.TipoCrimenId == escenaComparada.TipoCrimenId)
            {
                puntosTotales += PESO_TIPO_CRIMEN;
                resultado.Coincidencias.Add("Tipo de crimen coincidente");
            }

            // CRITERIO 2: Modus Operandi (25 puntos)
            if (escenaBase.ModusOperandiId > 0 && escenaComparada.ModusOperandiId > 0 &&
                escenaBase.ModusOperandiId == escenaComparada.ModusOperandiId)
            {
                puntosTotales += PESO_MODUS_OPERANDI;
                resultado.Coincidencias.Add("Modus operandi compatible");
            }

            // CRITERIO 3: Área Geográfica (20 puntos)
            if (escenaBase.AreaGeografica == escenaComparada.AreaGeografica)
            {
                puntosTotales += PESO_AREA_GEOGRAFICA;
                resultado.Coincidencias.Add("Área geográfica similar");
            }

            // CRITERIO 4: Horario (10 puntos)
            if (escenaBase.HorarioCrimen == escenaComparada.HorarioCrimen)
            {
                puntosTotales += PESO_HORARIO;
                resultado.Coincidencias.Add("Horario similar");
            }

            // CRITERIO 5: Evidencias Físicas (15 puntos) - Coeficiente de Jaccard
            double similitudEvidencias = CompararEvidencias(escenaBase.Evidencias, escenaComparada.Evidencias);
            puntosTotales += similitudEvidencias * PESO_EVIDENCIAS;
            
            if (similitudEvidencias > 0.5)
            {
                resultado.Coincidencias.Add($"Evidencia física similar ({Math.Round(similitudEvidencias * 100, 1)}%)");
            }
            else if (similitudEvidencias > 0)
            {
                resultado.Coincidencias.Add($"Alguna evidencia común ({Math.Round(similitudEvidencias * 100, 1)}%)");
            }

            // CRITERIO 6: Características Especiales (5 puntos)
            // Considera coincidencias en true y false (violencia, planificación, múltiples perpetradores)
            double puntosCaracteristicas = CalcularSimilitudCaracteristicas(escenaBase, escenaComparada);
            puntosTotales += puntosCaracteristicas;
            
            if (puntosCaracteristicas > 0)
            {
                resultado.Coincidencias.Add("Características especiales compatibles");
            }

            // Calcular porcentaje de similitud
            resultado.PorcentajeSimilitud = Math.Round((puntosTotales / puntosMaximos) * 100, 2);
            resultado.PorcentajeSimilitud = Math.Max(0, Math.Min(100, resultado.PorcentajeSimilitud));

            // Clasificar resultado
            if (resultado.PorcentajeSimilitud >= UMBRAL_CRIMEN_EN_SERIE)
                resultado.Clasificacion = ClasificacionCrimen.CrimenEnSerie;
            else if (resultado.PorcentajeSimilitud >= UMBRAL_CONEXION_PROBABLE)
                resultado.Clasificacion = ClasificacionCrimen.ConexionProbable;
            else
                resultado.Clasificacion = ClasificacionCrimen.SimilitudBaja;

            return resultado;
        }

        /// <summary>
        /// Calcula similitud de características especiales.
        /// Cuenta coincidencias cuando ambas escenas tienen el mismo valor (true o false).
        /// </summary>
        private double CalcularSimilitudCaracteristicas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            int coincidencias = 0;
            const int total = 3;

            if (escenaBase.UsoViolencia == escenaComparada.UsoViolencia) coincidencias++;
            if (escenaBase.ActoPlanificado == escenaComparada.ActoPlanificado) coincidencias++;
            if (escenaBase.MultiplesPerpetadores == escenaComparada.MultiplesPerpetadores) coincidencias++;

            return (coincidencias / (double)total) * PESO_CARACTERISTICAS;
        }

        /// <summary>
        /// Compara evidencias usando el coeficiente de Jaccard: J(A,B) = |A ∩ B| / |A ∪ B|
        /// Retorna valor entre 0 (sin similitud) y 1 (idénticas).
        /// </summary>
        private double CompararEvidencias(ICollection<Evidencia> evidencias1, ICollection<Evidencia> evidencias2)
        {
            if (evidencias1 == null || evidencias2 == null)
                return 0.0;

            if (evidencias1.Count == 0 && evidencias2.Count == 0)
                return 1.0;

            if (evidencias1.Count == 0 || evidencias2.Count == 0)
                return 0.0;

            // Usar HashSet para optimizar búsquedas O(1)
            var tipos1 = new HashSet<TipoEvidencia>(evidencias1.Select(e => e.TipoEvidencia));
            var tipos2 = new HashSet<TipoEvidencia>(evidencias2.Select(e => e.TipoEvidencia));

            int coincidencias = tipos1.Intersect(tipos2).Count();
            
            var union = new HashSet<TipoEvidencia>(tipos1);
            union.UnionWith(tipos2);
            int total = union.Count;

            return total == 0 ? 0.0 : (double)coincidencias / total;
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
