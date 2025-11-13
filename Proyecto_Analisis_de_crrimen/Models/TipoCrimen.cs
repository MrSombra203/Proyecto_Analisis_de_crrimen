using System;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Analisis_de_crimen.Models
{
    public class TipoCrimen
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con EscenasCrimen
        public virtual ICollection<EscenaCrimen> EscenasCrimen { get; set; } = new List<EscenaCrimen>();
    }
}

