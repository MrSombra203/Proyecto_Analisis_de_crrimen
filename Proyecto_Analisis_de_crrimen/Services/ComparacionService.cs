using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio principal del sistema que implementa el algoritmo de comparación de escenas de crímenes.
    /// Este es el CORE del proyecto y utiliza un sistema de puntuación multi-criterio para determinar
    /// el grado de similitud entre diferentes escenas delictivas.
    /// </summary>
    public class ComparacionService
    {
        /// <summary>
        /// Compara dos escenas de crímenes y calcula un porcentaje de similitud basado en múltiples criterios.
        /// 
        /// El algoritmo funciona mediante un sistema de puntuación donde cada criterio tiene un peso específico:
        /// - Tipo de Crimen: 25 puntos (25% del total)
        /// - Modus Operandi: 25 puntos (25% del total)
        /// - Área Geográfica: 20 puntos (20% del total)
        /// - Evidencias Físicas: 15 puntos (15% del total)
        /// - Horario: 10 puntos (10% del total)
        /// - Características Especiales: 5 puntos (5% del total)
        /// 
        /// Total: 100 puntos máximos
        /// </summary>
        /// <param name="escenaBase">La escena de referencia para la comparación</param>
        /// <param name="escenaComparada">La escena que se compara contra la base</param>
        /// <returns>Un objeto ComparacionResultado con el porcentaje de similitud y clasificación</returns>
        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            // Inicializar el objeto resultado con las escenas a comparar
            var resultado = new ComparacionResultado
            {
                EscenaBase = escenaBase,
                EscenaComparada = escenaComparada
            };

            // Variables para acumular puntos durante la comparación
            double puntosTotales = 0;    // Puntos obtenidos por coincidencias
            double puntosMaximos = 0;    // Puntos máximos posibles (siempre 100)

            // ============================================
            // CRITERIO 1: TIPO DE CRIMEN (25 puntos)
            // ============================================
            // Si ambas escenas son del mismo tipo de crimen, es un indicador fuerte de conexión
            puntosMaximos += 25;
            if (escenaBase.TipoCrimenId == escenaComparada.TipoCrimenId)
            {
                puntosTotales += 25;
                resultado.Coincidencias.Add("Tipo de crimen coincidente");
            }

            // ============================================
            // CRITERIO 2: ÁREA GEOGRÁFICA (20 puntos)
            // ============================================
            // Los crímenes en la misma zona geográfica pueden estar relacionados
            puntosMaximos += 20;
            if (escenaBase.AreaGeografica == escenaComparada.AreaGeografica)
            {
                puntosTotales += 20;
                resultado.Coincidencias.Add("Área geográfica similar");
            }

            // ============================================
            // CRITERIO 3: MODUS OPERANDI (25 puntos)
            // ============================================
            // El método utilizado es un indicador muy importante de autoría
            puntosMaximos += 25;
            if (escenaBase.ModusOperandiId == escenaComparada.ModusOperandiId)
            {
                puntosTotales += 25;
                resultado.Coincidencias.Add("Modus operandi compatible");
            }

            // ============================================
            // CRITERIO 4: HORARIO DEL CRIMEN (10 puntos)
            // ============================================
            // Los criminales en serie suelen operar en horarios similares
            puntosMaximos += 10;
            if (escenaBase.HorarioCrimen == escenaComparada.HorarioCrimen)
            {
                puntosTotales += 10;
                resultado.Coincidencias.Add("Horario similar");
            }

            // ============================================
            // CRITERIO 5: EVIDENCIAS FÍSICAS (15 puntos)
            // ============================================
            // Compara los tipos de evidencias encontradas usando el coeficiente de Jaccard
            puntosMaximos += 15;
            double similitudEvidencias = CompararEvidencias(escenaBase.Evidencias, escenaComparada.Evidencias);
            puntosTotales += similitudEvidencias * 15; // Multiplicar por el peso del criterio
            if (similitudEvidencias > 0.5) // Si más del 50% de las evidencias coinciden
            {
                resultado.Coincidencias.Add("Evidencia física similar");
            }

            // ============================================
            // CRITERIO 6: CARACTERÍSTICAS ESPECIALES (5 puntos)
            // ============================================
            // Evalúa características booleanas: violencia, planificación, múltiples perpetradores
            puntosMaximos += 5;
            int caracteristicasCoincidentes = 0;
            int totalCaracteristicas = 3; // Total de características a evaluar

            // Solo cuenta como coincidencia si AMBAS escenas tienen la característica activa
            if (escenaBase.UsoViolencia == escenaComparada.UsoViolencia && escenaBase.UsoViolencia)
                caracteristicasCoincidentes++;
            
            if (escenaBase.ActoPlanificado == escenaComparada.ActoPlanificado && escenaBase.ActoPlanificado)
                caracteristicasCoincidentes++;
            
            if (escenaBase.MultiplesPerpetadores == escenaComparada.MultiplesPerpetadores && escenaBase.MultiplesPerpetadores)
                caracteristicasCoincidentes++;

            // Calcular puntos proporcionales: (coincidencias / total) * puntos máximos
            puntosTotales += (caracteristicasCoincidentes / (double)totalCaracteristicas) * 5;

            // ============================================
            // CÁLCULO DEL PORCENTAJE DE SIMILITUD
            // ============================================
            // Fórmula: (puntos obtenidos / puntos máximos) * 100
            resultado.PorcentajeSimilitud = Math.Round((puntosTotales / puntosMaximos) * 100, 2);

            // ============================================
            // CLASIFICACIÓN DEL RESULTADO
            // ============================================
            // Basado en el porcentaje de similitud, clasificamos el resultado:
            if (resultado.PorcentajeSimilitud >= 75)
                resultado.Clasificacion = ClasificacionCrimen.CrimenEnSerie;      // Alta probabilidad de conexión
            else if (resultado.PorcentajeSimilitud >= 60)
                resultado.Clasificacion = ClasificacionCrimen.ConexionProbable;  // Posible relación
            else
                resultado.Clasificacion = ClasificacionCrimen.SimilitudBaja;     // Baja probabilidad

            return resultado;
        }

        /// <summary>
        /// Compara dos colecciones de evidencias físicas usando el coeficiente de Jaccard.
        /// 
        /// El coeficiente de Jaccard mide la similitud entre dos conjuntos:
        /// J(A,B) = |A ∩ B| / |A ∪ B|
        /// 
        /// Donde:
        /// - A ∩ B = elementos comunes entre ambos conjuntos
        /// - A ∪ B = todos los elementos únicos de ambos conjuntos
        /// 
        /// Ejemplo:
        /// Escena A tiene: [Huellas, Sangre, Vidrio]
        /// Escena B tiene: [Huellas, Vidrio, Fibras]
        /// Coincidencias: [Huellas, Vidrio] = 2
        /// Total único: [Huellas, Sangre, Vidrio, Fibras] = 4
        /// Similitud = 2/4 = 0.5 (50%)
        /// </summary>
        /// <param name="evidencias1">Primera colección de evidencias</param>
        /// <param name="evidencias2">Segunda colección de evidencias</param>
        /// <returns>Un valor entre 0 y 1 representando la similitud (0 = nada similar, 1 = idénticas)</returns>
        private double CompararEvidencias(ICollection<Evidencia> evidencias1, ICollection<Evidencia> evidencias2)
        {
            // Validación: si alguna colección es nula o vacía, no hay similitud
            if (evidencias1 == null || evidencias2 == null ||
                evidencias1.Count == 0 || evidencias2.Count == 0)
                return 0;

            // Extraer solo los tipos de evidencias (enum) de cada colección
            var tipos1 = evidencias1.Select(e => e.TipoEvidencia).ToList();
            var tipos2 = evidencias2.Select(e => e.TipoEvidencia).ToList();

            // Calcular intersección: tipos que aparecen en AMBAS colecciones
            int coincidencias = tipos1.Intersect(tipos2).Count();
            
            // Calcular unión: todos los tipos únicos de ambas colecciones
            int total = tipos1.Union(tipos2).Distinct().Count();

            // Aplicar fórmula de Jaccard: coincidencias / total único
            return (double)coincidencias / total;
        }

        /// <summary>
        /// Busca todas las escenas similares a una escena base dentro de una colección de escenas.
        /// 
        /// Este método compara la escena base contra todas las demás escenas y retorna
        /// solo aquellas que superen el umbral mínimo de similitud (por defecto 60%).
        /// 
        /// El resultado se ordena de mayor a menor similitud para facilitar el análisis.
        /// </summary>
        /// <param name="escenaBase">La escena de referencia para buscar similares</param>
        /// <param name="todasLasEscenas">Lista completa de escenas en el sistema</param>
        /// <param name="umbralMinimo">Porcentaje mínimo de similitud requerido (default: 60%)</param>
        /// <returns>Lista de resultados de comparación ordenados por similitud descendente</returns>
        public List<ComparacionResultado> BuscarEscenasSimilares(EscenaCrimen escenaBase, List<EscenaCrimen> todasLasEscenas, double umbralMinimo = 60)
        {
            var resultados = new List<ComparacionResultado>();

            // Iterar sobre todas las escenas disponibles
            foreach (var escena in todasLasEscenas)
            {
                // Omitir la escena base (no se compara consigo misma)
                if (escena.Id == escenaBase.Id) 
                    continue;

                // Realizar la comparación usando el algoritmo principal
                var comparacion = CompararEscenas(escenaBase, escena);

                // Solo agregar si supera el umbral mínimo de similitud
                if (comparacion.PorcentajeSimilitud >= umbralMinimo)
                {
                    resultados.Add(comparacion);
                }
            }

            // Ordenar por porcentaje de similitud descendente (las más similares primero)
            return resultados.OrderByDescending(r => r.PorcentajeSimilitud).ToList();
        }

        /// <summary>
        /// Detecta grupos de crímenes que forman series criminales.
        /// 
        /// Un crimen en serie se define como un grupo de 3 o más crímenes relacionados
        /// con un umbral de similitud del 75% o superior.
        /// 
        /// Algoritmo:
        /// 1. Para cada escena no procesada, busca escenas similares (≥75% similitud)
        /// 2. Si encuentra 2 o más escenas similares, forma un grupo (serie)
        /// 3. Marca todas las escenas del grupo como procesadas para evitar duplicados
        /// 4. Retorna todos los grupos detectados
        /// 
        /// Este método es útil para identificar patrones criminales que pueden indicar
        /// un mismo autor o grupo de autores operando de manera sistemática.
        /// </summary>
        /// <param name="todasLasEscenas">Lista completa de escenas en el sistema</param>
        /// <returns>Lista de grupos (series) donde cada grupo es una lista de escenas relacionadas</returns>
        public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
        {
            var series = new List<List<EscenaCrimen>>();
            var procesadas = new HashSet<int>(); // Para evitar procesar la misma escena múltiples veces

            // Iterar sobre todas las escenas
            foreach (var escena in todasLasEscenas)
            {
                // Si ya fue procesada como parte de otra serie, omitirla
                if (procesadas.Contains(escena.Id)) 
                    continue;

                // Buscar escenas similares con umbral alto (75% = CrimenEnSerie)
                var similares = BuscarEscenasSimilares(escena, todasLasEscenas, 75);

                // Se requiere al menos 2 escenas similares para formar una serie
                // (la escena base + 2 similares = mínimo 3 crímenes)
                if (similares.Count >= 2)
                {
                    // Crear el grupo de serie: escena base + todas las similares
                    var serie = new List<EscenaCrimen> { escena };
                    serie.AddRange(similares.Select(s => s.EscenaComparada));

                    // Marcar todas las escenas de esta serie como procesadas
                    foreach (var e in serie)
                    {
                        procesadas.Add(e.Id);
                    }

                    // Agregar la serie detectada a la lista de resultados
                    series.Add(serie);
                }
            }

            return series;
        }
    }
}