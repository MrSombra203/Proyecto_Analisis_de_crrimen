using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Analisis_de_crimen.Models
{
    // Representa una escena de crimen registrada en el sistema
    // Esta es la clase principal del sistema, contiene toda la información de un crimen
    // que se usa para comparar con otros y detectar posibles conexiones
    public class EscenaCrimen
    {
        // ID único de la escena (clave primaria)
        [Key]
        public int Id { get; set; }

        // Cuándo ocurrió el crimen
        // Importante para analizar patrones temporales en crímenes en serie
        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha del Crimen")]
        public DateTime FechaCrimen { get; set; }

        // Dónde ocurrió el crimen (dirección, coordenadas, etc.)
        // Máximo 200 caracteres
        [Required(ErrorMessage = "La ubicación es obligatoria")]
        [StringLength(200)]
        [Display(Name = "Ubicación Geográfica")]
        public string Ubicacion { get; set; } = string.Empty;

        // ID del tipo de crimen (homicidio, robo, etc.)
        // Es una clave foránea que apunta a la tabla TiposCrimen
        [Required(ErrorMessage = "El tipo de crimen es obligatorio")]
        [Display(Name = "Tipo de Crimen")]
        public int TipoCrimenId { get; set; }

        // Objeto completo del tipo de crimen
        // Entity Framework lo carga cuando usamos Include() en las consultas
        [Display(Name = "Tipo de Crimen")]
        public virtual TipoCrimen? TipoCrimen { get; set; }

        // Área geográfica donde ocurrió (Centro, Norte, Sur, etc.)
        // Se usa para agrupar crímenes por zona
        [Required(ErrorMessage = "El área geográfica es obligatoria")]
        [Display(Name = "Área Geográfica")]
        public AreaGeografica AreaGeografica { get; set; }

        // ID del modus operandi (cómo se cometió el crimen)
        [Required(ErrorMessage = "El modus operandi es obligatorio")]
        [Display(Name = "Modus Operandi")]
        public int ModusOperandiId { get; set; }

        // Objeto completo del modus operandi
        [Display(Name = "Modus Operandi")]
        public virtual ModusOperandi? ModusOperandi { get; set; }

        // Horario del día en que ocurrió (Madrugada, Mañana, Tarde, Noche)
        // Útil para detectar patrones temporales
        [Required(ErrorMessage = "El horario es obligatorio")]
        [Display(Name = "Horario del Crimen")]
        public HorarioCrimen HorarioCrimen { get; set; }

        // Descripción adicional de la escena (opcional)
        // Máximo 2000 caracteres
        [Display(Name = "Descripción Detallada")]
        [StringLength(2000)]
        public string? DescripcionDetallada { get; set; }

        // Lista de evidencias encontradas en esta escena
        // Una escena puede tener varias evidencias
        public virtual ICollection<Evidencia> Evidencias { get; set; }

        // ============================================
        // Características especiales
        // Estas propiedades se usan en el algoritmo de comparación
        // ============================================

        // Si se usó violencia durante el crimen
        public bool UsoViolencia { get; set; }

        // Si el crimen fue planificado de antemano
        public bool ActoPlanificado { get; set; }

        // Si participaron varios perpetradores
        public bool MultiplesPerpetadores { get; set; }

        // Si no se conoce quién lo hizo
        public bool AutoriaDesconocida { get; set; }

        // ============================================
        // Metadatos del registro
        // ============================================

        // Cuándo se registró esta escena en el sistema
        // Se establece automáticamente al crear
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }

        // Quién registró esta escena
        [Display(Name = "Usuario Registro")]
        public string? UsuarioRegistro { get; set; }

        // Constructor: inicializa la lista de evidencias y la fecha de registro
        public EscenaCrimen()
        {
            Evidencias = new List<Evidencia>();
            FechaRegistro = DateTime.Now;
        }
    }

    // Áreas geográficas de la ciudad
    // Se usa para agrupar crímenes por zona
    public enum AreaGeografica
    {
        Centro,
        Norte,
        Sur,
        Este,
        Oeste
    }

    // Horarios del día en que puede ocurrir un crimen
    // Cada uno tiene un nombre más descriptivo que se muestra en la interfaz
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