using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services.Strategies
{
    /// <summary>
    /// Estrategia de comparación enfocada en aspectos geográficos y temporales
    /// Prioriza ubicación y horario sobre otros criterios
    /// Aplica OCP: Nueva estrategia sin modificar código existente
    /// </summary>
    public class ComparacionGeograficaStrategy : IComparacionStrategy
    {
        public string Nombre => "Comparación Geográfica y Temporal";

        // PESOS AJUSTADOS: Mayor énfasis en geografía y tiempo
        private const double PESO_AREA_GEOGRAFICA = 40.0;  // Aumentado de 20 a 40
        private const double PESO_HORARIO = 25.0;         // Aumentado de 10 a 25
        private const double PESO_TIPO_CRIMEN = 20.0;     // Reducido de 25 a 20
        private const double PESO_MODUS_OPERANDI = 10.0;  // Reducido de 25 a 10
        private const double PESO_EVIDENCIAS = 5.0;       // Reducido de 15 a 5

        private const double UMBRAL_CRIMEN_EN_SERIE = 75.0;
        private const double UMBRAL_CONEXION_PROBABLE = 60.0;

        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            if (escenaBase == null)
                throw new ArgumentNullException(nameof(escenaBase));

            if (escenaComparada == null)
                throw new ArgumentNullException(nameof(escenaComparada));

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

            // Prioridad 1: Área Geográfica (40 puntos)
            if (escenaBase.AreaGeografica == escenaComparada.AreaGeografica)
            {
                puntosTotales += PESO_AREA_GEOGRAFICA;
                resultado.Coincidencias.Add("Misma área geográfica");
            }

            // Prioridad 2: Horario (25 puntos)
            if (escenaBase.HorarioCrimen == escenaComparada.HorarioCrimen)
            {
                puntosTotales += PESO_HORARIO;
                resultado.Coincidencias.Add("Mismo horario");
            }

            // Prioridad 3: Tipo de Crimen (20 puntos)
            if (escenaBase.TipoCrimenId > 0 && escenaComparada.TipoCrimenId > 0 &&
                escenaBase.TipoCrimenId == escenaComparada.TipoCrimenId)
            {
                puntosTotales += PESO_TIPO_CRIMEN;
                resultado.Coincidencias.Add("Tipo de crimen coincidente");
            }

            // Prioridad 4: Modus Operandi (10 puntos)
            if (escenaBase.ModusOperandiId > 0 && escenaComparada.ModusOperandiId > 0 &&
                escenaBase.ModusOperandiId == escenaComparada.ModusOperandiId)
            {
                puntosTotales += PESO_MODUS_OPERANDI;
                resultado.Coincidencias.Add("Modus operandi similar");
            }

            // Prioridad 5: Evidencias (5 puntos)
            double similitudEvidencias = CompararEvidencias(escenaBase.Evidencias, escenaComparada.Evidencias);
            puntosTotales += similitudEvidencias * PESO_EVIDENCIAS;

            if (similitudEvidencias > 0.5)
            {
                resultado.Coincidencias.Add($"Evidencias similares ({Math.Round(similitudEvidencias * 100, 1)}%)");
            }

            // Calcular porcentaje
            resultado.PorcentajeSimilitud = Math.Round((puntosTotales / puntosMaximos) * 100, 2);
            resultado.PorcentajeSimilitud = Math.Max(0, Math.Min(100, resultado.PorcentajeSimilitud));

            // Clasificar
            if (resultado.PorcentajeSimilitud >= UMBRAL_CRIMEN_EN_SERIE)
                resultado.Clasificacion = ClasificacionCrimen.CrimenEnSerie;
            else if (resultado.PorcentajeSimilitud >= UMBRAL_CONEXION_PROBABLE)
                resultado.Clasificacion = ClasificacionCrimen.ConexionProbable;
            else
                resultado.Clasificacion = ClasificacionCrimen.SimilitudBaja;

            return resultado;
        }

        private double CompararEvidencias(ICollection<Evidencia> evidencias1, ICollection<Evidencia> evidencias2)
        {
            if (evidencias1 == null || evidencias2 == null)
                return 0.0;

            if (evidencias1.Count == 0 && evidencias2.Count == 0)
                return 1.0;

            if (evidencias1.Count == 0 || evidencias2.Count == 0)
                return 0.0;

            var tipos1 = new HashSet<TipoEvidencia>(evidencias1.Select(e => e.TipoEvidencia));
            var tipos2 = new HashSet<TipoEvidencia>(evidencias2.Select(e => e.TipoEvidencia));

            int coincidencias = tipos1.Intersect(tipos2).Count();
            var union = new HashSet<TipoEvidencia>(tipos1);
            union.UnionWith(tipos2);
            int total = union.Count;

            return total == 0 ? 0.0 : (double)coincidencias / total;
        }
    }
}

