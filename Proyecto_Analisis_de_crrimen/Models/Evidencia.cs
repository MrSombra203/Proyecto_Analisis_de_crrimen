using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Analisis_de_crimen.Models
{
    // Representa una evidencia encontrada en una escena de crimen
    // Las evidencias son cosas físicas o rastros que se encuentran en la escena
    // y que pueden ayudar a conectar diferentes crímenes
    // El sistema las usa para comparar escenas con el coeficiente de Jaccard
    public class Evidencia
    {
        // ID único de la evidencia
        [Key]
        public int Id { get; set; }

        // ID de la escena a la que pertenece esta evidencia
        // Varias evidencias pueden pertenecer a una misma escena
        [ForeignKey("EscenaCrimen")]
        public int EscenaCrimenId { get; set; }

        // Objeto completo de la escena
        // Entity Framework lo carga cuando usamos Include() en las consultas
        public virtual EscenaCrimen EscenaCrimen { get; set; }

        // Qué tipo de evidencia es (vidrio roto, huellas, sangre, etc.)
        [Required]
        public TipoEvidencia TipoEvidencia { get; set; }

        // Descripción adicional de la evidencia (opcional)
        // Puede incluir dónde se encontró, su estado, características, etc.
        // Máximo 500 caracteres
        [StringLength(500)]
        public string? Descripcion { get; set; }
    }

    // Tipos de evidencias que pueden encontrarse en una escena
    // Cada uno tiene un nombre más descriptivo que se muestra en la interfaz
    // Se usan para calcular la similitud entre escenas
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