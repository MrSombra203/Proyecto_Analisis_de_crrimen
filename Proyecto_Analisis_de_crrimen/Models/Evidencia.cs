using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Proyecto_Analisis_de_crimen.Models
{
    public class Evidencia
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("EscenaCrimen")]
        public int EscenaCrimenId { get; set; }
        public virtual EscenaCrimen EscenaCrimen { get; set; }

        [Required]
        public TipoEvidencia TipoEvidencia { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }

    public enum TipoEvidencia
    {
        [Display(Name = "Vidrio Roto")]
        VidrioRoto,
        [Display(Name = "Huellas Dactilares")]
        HuellasDactilares,
        [Display(Name = "Sangre")]
        Sangre,
        [Display(Name = "Cabellos")]
        Cabellos,
        [Display(Name = "Fibras")]
        Fibras,
        [Display(Name = "Arma de Fuego")]
        ArmaFuego
    }
}