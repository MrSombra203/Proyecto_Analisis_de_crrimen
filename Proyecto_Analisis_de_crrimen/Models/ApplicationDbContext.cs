using Microsoft.EntityFrameworkCore;

namespace Proyecto_Analisis_de_crimen.Models
{
    /// <summary>
    /// Contexto de base de datos de Entity Framework Core.
    /// 
    /// Este contexto representa la conexión con la base de datos y define:
    /// - Las tablas (DbSet) que se pueden consultar
    /// - Las relaciones entre entidades
    /// - Las configuraciones de esquema (índices, constraints, valores por defecto)
    /// 
    /// Entity Framework Core usa este contexto para:
    /// - Generar las consultas SQL automáticamente
    /// - Mapear objetos C# a filas de la base de datos (ORM)
    /// - Gestionar las relaciones entre tablas
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe las opciones de configuración del contexto
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============================================
        // DEFINICIÓN DE TABLAS (DbSet)
        // ============================================
        // Cada DbSet representa una tabla en la base de datos.
        // Entity Framework Core usa estos DbSet para realizar operaciones CRUD.

        /// <summary>Tabla de roles del sistema (Administrador, Usuario, etc.)</summary>
        public DbSet<Rol> Roles { get; set; }

        /// <summary>Tabla de usuarios del sistema</summary>
        public DbSet<Usuario> Usuarios { get; set; }

        /// <summary>Catálogo de tipos de crímenes (Robo, Homicidio, etc.)</summary>
        public DbSet<TipoCrimen> TiposCrimen { get; set; }

        /// <summary>Catálogo de modus operandi (métodos utilizados en los crímenes)</summary>
        public DbSet<ModusOperandi> ModusOperandi { get; set; }

        /// <summary>Tabla principal: escenas de crímenes registradas</summary>
        public DbSet<EscenaCrimen> EscenasCrimen { get; set; }

        /// <summary>Tabla de evidencias físicas relacionadas con escenas</summary>
        public DbSet<Evidencia> Evidencias { get; set; }

        /// <summary>
        /// Método que se ejecuta cuando Entity Framework Core está creando el modelo de datos.
        /// 
        /// Aquí se configuran:
        /// - Nombres de tablas y columnas
        /// - Restricciones (required, max length)
        /// - Relaciones entre tablas (foreign keys)
        /// - Índices para optimizar consultas
        /// - Valores por defecto
        /// 
        /// Esta configuración se aplica cuando se crea la base de datos o se ejecutan migraciones.
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo que permite configurar las entidades</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CONFIGURACIÓN DE LA TABLA ROLES
            // ============================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Roles");           // Nombre de la tabla en la BD
                entity.HasKey(e => e.Id);         // Definir clave primaria

                // Configurar propiedades con restricciones
                entity.Property(e => e.Nombre)
                    .IsRequired()                  // Campo obligatorio (NOT NULL)
                    .HasMaxLength(50);            // Longitud máxima de 50 caracteres

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(200);           // Longitud máxima (campo opcional)

                // Valor por defecto: usar función GETDATE() de SQL Server
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                // Índice único: garantiza que no haya dos roles con el mismo nombre
                // Esto mejora el rendimiento de búsquedas y previene duplicados
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // ============================================
            // CONFIGURACIÓN DE LA TABLA USUARIOS
            // ============================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);

                // Propiedades con restricciones de longitud y obligatoriedad
                entity.Property(e => e.NombreUsuario)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.NombreCompleto)
                    .IsRequired()
                    .HasMaxLength(100);

                // Valor por defecto: los usuarios se crean como activos
                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                // ============================================
                // RELACIÓN CON ROLES (Foreign Key)
                // ============================================
                // Un Usuario tiene UN Rol, un Rol tiene MUCHOS Usuarios (1:N)
                // DeleteBehavior.Restrict: No permite eliminar un Rol si tiene Usuarios asignados
                // Esto previene la pérdida de integridad referencial
                entity.HasOne(e => e.Rol)              // Un usuario tiene un rol
                    .WithMany(r => r.Usuarios)          // Un rol tiene muchos usuarios
                    .HasForeignKey(e => e.RolId)        // Clave foránea: RolId
                    .OnDelete(DeleteBehavior.Restrict); // Restricción: no eliminar si hay usuarios

                // ============================================
                // ÍNDICES PARA OPTIMIZACIÓN
                // ============================================
                // Los índices mejoran el rendimiento de las consultas frecuentes
                
                entity.HasIndex(e => e.NombreUsuario).IsUnique();  // Búsqueda rápida y único
                entity.HasIndex(e => e.Email).IsUnique();          // Búsqueda rápida y único
                entity.HasIndex(e => e.RolId);                     // Búsqueda por rol (filtros)
                entity.HasIndex(e => e.Activo);                  // Búsqueda por estado (filtros)
            });

            // Configuración de la tabla TiposCrimen
            modelBuilder.Entity<TipoCrimen>(entity =>
            {
                entity.ToTable("TiposCrimen");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                // Índices
                entity.HasIndex(e => e.Nombre);
                entity.HasIndex(e => e.Activo);
            });

            // Configuración de la tabla ModusOperandi
            modelBuilder.Entity<ModusOperandi>(entity =>
            {
                entity.ToTable("ModusOperandi");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                // Índices
                entity.HasIndex(e => e.Nombre);
                entity.HasIndex(e => e.Activo);
            });

            // ============================================
            // CONFIGURACIÓN DE LA TABLA ESCENASCRIMEN
            // ============================================
            // Esta es la tabla PRINCIPAL del sistema que almacena las escenas de crímenes
            modelBuilder.Entity<EscenaCrimen>(entity =>
            {
                entity.ToTable("EscenasCrimen");
                entity.HasKey(e => e.Id);

                // Propiedades con restricciones
                entity.Property(e => e.Ubicacion)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.DescripcionDetallada)
                    .HasMaxLength(2000);  // Campo opcional para descripciones extensas

                entity.Property(e => e.UsuarioRegistro)
                    .HasMaxLength(100);

                // Fecha de registro automática al crear el registro
                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("GETDATE()");

                // ============================================
                // VALORES POR DEFECTO PARA CAMPOS BOOLEANOS
                // ============================================
                // Todos los campos booleanos se inicializan en false por defecto
                entity.Property(e => e.UsoViolencia)
                    .HasDefaultValue(false);

                entity.Property(e => e.ActoPlanificado)
                    .HasDefaultValue(false);

                entity.Property(e => e.MultiplesPerpetadores)
                    .HasDefaultValue(false);

                entity.Property(e => e.AutoriaDesconocida)
                    .HasDefaultValue(false);

                // ============================================
                // RELACIONES CON CATÁLOGOS (Foreign Keys)
                // ============================================
                // Una EscenaCrimen tiene UN TipoCrimen, un TipoCrimen tiene MUCHAS EscenasCrimen
                // DeleteBehavior.Restrict: No permite eliminar catálogos si hay escenas que los usan
                entity.HasOne(e => e.TipoCrimen)
                    .WithMany(tc => tc.EscenasCrimen)
                    .HasForeignKey(e => e.TipoCrimenId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Una EscenaCrimen tiene UN ModusOperandi, un ModusOperandi tiene MUCHAS EscenasCrimen
                entity.HasOne(e => e.ModusOperandi)
                    .WithMany(mo => mo.EscenasCrimen)
                    .HasForeignKey(e => e.ModusOperandiId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ============================================
                // ÍNDICES PARA OPTIMIZACIÓN DE CONSULTAS
                // ============================================
                // Estos índices mejoran el rendimiento de las consultas más frecuentes
                
                entity.HasIndex(e => e.FechaCrimen);        // Búsquedas por fecha del crimen
                entity.HasIndex(e => e.TipoCrimenId);       // Filtros por tipo de crimen
                entity.HasIndex(e => e.AreaGeografica);    // Filtros por área geográfica
                entity.HasIndex(e => e.ModusOperandiId);    // Filtros por modus operandi
                entity.HasIndex(e => e.FechaRegistro);     // Ordenamiento por fecha de registro

                // ============================================
                // ÍNDICE COMPUESTO PARA COMPARACIONES
                // ============================================
                // Este índice compuesto optimiza las consultas de comparación que buscan
                // escenas similares basándose en múltiples criterios simultáneamente.
                // Es especialmente útil para el ComparacionService que busca escenas similares.
                entity.HasIndex(e => new { e.TipoCrimenId, e.AreaGeografica, e.ModusOperandiId, e.HorarioCrimen })
                    .HasDatabaseName("IX_EscenasCrimen_Comparacion");
            });

            // ============================================
            // CONFIGURACIÓN DE LA TABLA EVIDENCIAS
            // ============================================
            // Las evidencias son dependientes de las escenas (no existen sin una escena)
            modelBuilder.Entity<Evidencia>(entity =>
            {
                entity.ToTable("Evidencias");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);  // Campo opcional para descripciones adicionales

                // ============================================
                // RELACIÓN CON ESCENACRIMEN (CASCADE DELETE)
                // ============================================
                // Una Evidencia pertenece a UNA EscenaCrimen, una EscenaCrimen tiene MUCHAS Evidencias
                // DeleteBehavior.Cascade: Si se elimina una escena, se eliminan automáticamente sus evidencias
                // Esto tiene sentido porque las evidencias no tienen significado sin su escena asociada
                entity.HasOne(e => e.EscenaCrimen)
                    .WithMany(ec => ec.Evidencias)
                    .HasForeignKey(e => e.EscenaCrimenId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice en clave foránea para optimizar consultas que buscan evidencias de una escena
                entity.HasIndex(e => e.EscenaCrimenId);
            });
        }
    }
}