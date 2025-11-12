using Proyecto_Analisis_de_crrimen.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Analisis_de_crimen.Models
{
    public class EscenaCrimen
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha del Crimen")]
        public DateTime FechaCrimen { get; set; }

        [Required(ErrorMessage = "La ubicación es obligatoria")]
        [StringLength(200)]
        [Display(Name = "Ubicación Geográfica")]
        public string Ubicacion { get; set; }

        [Required(ErrorMessage = "El tipo de crimen es obligatorio")]
        [Display(Name = "Tipo de Crimen")]
        public TipoCrimen TipoCrimen { get; set; }

        [Required(ErrorMessage = "El área geográfica es obligatoria")]
        [Display(Name = "Área Geográfica")]
        public AreaGeografica AreaGeografica { get; set; }

        [Required(ErrorMessage = "El modus operandi es obligatorio")]
        [Display(Name = "Modus Operandi")]
        public ModusOperandi ModusOperandi { get; set; }

        [Required(ErrorMessage = "El horario es obligatorio")]
        [Display(Name = "Horario del Crimen")]
        public HorarioCrimen HorarioCrimen { get; set; }

        [Display(Name = "Descripción Detallada")]
        [StringLength(2000)]
        public string DescripcionDetallada { get; set; }

        public virtual ICollection<Evidencia> Evidencias { get; set; }

        public bool UsoViolencia { get; set; }
        public bool ActoPlanificado { get; set; }
        public bool MultiplesPerpetadores { get; set; }
        public bool AutoriaDesconocida { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Usuario Registro")]
        public string UsuarioRegistro { get; set; }

        public EscenaCrimen()
        {
            Evidencias = new List<Evidencia>();
            FechaRegistro = DateTime.Now;
        }
    }

    public enum TipoCrimen
    {
        [Display(Name = "Robo")]
        Robo,
        [Display(Name = "Asalto a Mano Armada")]
        AsaltoManoArmada,
        [Display(Name = "Homicidio")]
        Homicidio,
        [Display(Name = "Allanamiento")]
        Allanamiento
    }

    public enum AreaGeografica
    {
        Centro,
        Norte,
        Sur,
        Este,
        Oeste
    }

    public enum ModusOperandi
    {
        [Display(Name = "Acceso por Ventana")]
        AccesoVentana,
        [Display(Name = "Acceso por Puerta")]
        AccesoPuerta,
        [Display(Name = "Escalada")]
        Escalada,
        [Display(Name = "Uso de Fuerza Bruta")]
        FuerzaBruta,
        [Display(Name = "Engaño/Coerción")]
        EngañoCoercion
    }

    public enum HorarioCrimen
    {
        [Display(Name = "Madrugada (00:00-06:00)")]
        Madrugada,
        [Display(Name = "Mañana (06:00-12:00)")]
        Mañana,
        [Display(Name = "Tarde (12:00-18:00)")]
        Tarde,
        [Display(Name = "Noche (18:00-24:00)")]
        Noche
    }
}