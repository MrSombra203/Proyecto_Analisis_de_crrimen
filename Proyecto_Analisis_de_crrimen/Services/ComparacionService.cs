using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    public class ComparacionService
    {
        public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
        {
            var resultado = new ComparacionResultado
            {
                EscenaBase = escenaBase,
                EscenaComparada = escenaComparada
            };

            double puntosTotales = 0;
            double puntosMaximos = 0;

            puntosMaximos += 25;
            if (escenaBase.TipoCrimenId == escenaComparada.TipoCrimenId)
            {
                puntosTotales += 25;
                resultado.Coincidencias.Add("Tipo de crimen coincidente");
            }

            puntosMaximos += 20;
            if (escenaBase.AreaGeografica == escenaComparada.AreaGeografica)
            {
                puntosTotales += 20;
                resultado.Coincidencias.Add("Área geográfica similar");
            }

            puntosMaximos += 25;
            if (escenaBase.ModusOperandiId == escenaComparada.ModusOperandiId)
            {
                puntosTotales += 25;
                resultado.Coincidencias.Add("Modus operandi compatible");
            }

            puntosMaximos += 10;
            if (escenaBase.HorarioCrimen == escenaComparada.HorarioCrimen)
            {
                puntosTotales += 10;
                resultado.Coincidencias.Add("Horario similar");
            }

            puntosMaximos += 15;
            double similitudEvidencias = CompararEvidencias(escenaBase.Evidencias, escenaComparada.Evidencias);
            puntosTotales += similitudEvidencias * 15;
            if (similitudEvidencias > 0.5)
            {
                resultado.Coincidencias.Add("Evidencia física similar");
            }

            puntosMaximos += 5;
            int caracteristicasCoincidentes = 0;
            if (escenaBase.UsoViolencia == escenaComparada.UsoViolencia && escenaBase.UsoViolencia) caracteristicasCoincidentes++;
            if (escenaBase.ActoPlanificado == escenaComparada.ActoPlanificado && escenaBase.ActoPlanificado) caracteristicasCoincidentes++;
            if (escenaBase.MultiplesPerpetadores == escenaComparada.MultiplesPerpetadores && escenaBase.MultiplesPerpetadores) caracteristicasCoincidentes++;

            puntosTotales += (caracteristicasCoincidentes / 3.0) * 5;

            resultado.PorcentajeSimilitud = Math.Round((puntosTotales / puntosMaximos) * 100, 2);

            if (resultado.PorcentajeSimilitud >= 75)
                resultado.Clasificacion = ClasificacionCrimen.CrimenEnSerie;
            else if (resultado.PorcentajeSimilitud >= 60)
                resultado.Clasificacion = ClasificacionCrimen.ConexionProbable;
            else
                resultado.Clasificacion = ClasificacionCrimen.SimilitudBaja;

            return resultado;
        }

        private double CompararEvidencias(ICollection<Evidencia> evidencias1, ICollection<Evidencia> evidencias2)
        {
            if (evidencias1 == null || evidencias2 == null ||
                evidencias1.Count == 0 || evidencias2.Count == 0)
                return 0;

            var tipos1 = evidencias1.Select(e => e.TipoEvidencia).ToList();
            var tipos2 = evidencias2.Select(e => e.TipoEvidencia).ToList();

            int coincidencias = tipos1.Intersect(tipos2).Count();
            int total = tipos1.Union(tipos2).Distinct().Count();

            return (double)coincidencias / total;
        }

        public List<ComparacionResultado> BuscarEscenasSimilares(EscenaCrimen escenaBase, List<EscenaCrimen> todasLasEscenas, double umbralMinimo = 60)
        {
            var resultados = new List<ComparacionResultado>();

            foreach (var escena in todasLasEscenas)
            {
                if (escena.Id == escenaBase.Id) continue;

                var comparacion = CompararEscenas(escenaBase, escena);

                if (comparacion.PorcentajeSimilitud >= umbralMinimo)
                {
                    resultados.Add(comparacion);
                }
            }

            return resultados.OrderByDescending(r => r.PorcentajeSimilitud).ToList();
        }

        public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
        {
            var series = new List<List<EscenaCrimen>>();
            var procesadas = new HashSet<int>();

            foreach (var escena in todasLasEscenas)
            {
                if (procesadas.Contains(escena.Id)) continue;

                var similares = BuscarEscenasSimilares(escena, todasLasEscenas, 75);

                if (similares.Count >= 2)
                {
                    var serie = new List<EscenaCrimen> { escena };
                    serie.AddRange(similares.Select(s => s.EscenaComparada));

                    foreach (var e in serie)
                    {
                        procesadas.Add(e.Id);
                    }

                    series.Add(serie);
                }
            }

            return series;
        }
    }
}