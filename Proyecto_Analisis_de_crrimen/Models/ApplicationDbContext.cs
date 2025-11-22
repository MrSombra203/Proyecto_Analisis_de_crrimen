using Microsoft.EntityFrameworkCore;

namespace Proyecto_Analisis_de_crimen.Models
{
    /// <summary>
    /// Contexto de base de datos EF Core. Define tablas (DbSet), relaciones, índices y constraints.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>Tabla de roles (Administrador, Usuario)</summary>
        public DbSet<Rol> Roles { get; set; }

        /// <summary>Tabla de usuarios</summary>
        public DbSet<Usuario> Usuarios { get; set; }

        /// <summary>Catálogo de tipos de crímenes</summary>
        public DbSet<TipoCrimen> TiposCrimen { get; set; }

        /// <summary>Catálogo de modus operandi</summary>
        public DbSet<ModusOperandi> ModusOperandi { get; set; }

        /// <summary>Tabla principal: escenas de crímenes</summary>
        public DbSet<EscenaCrimen> EscenasCrimen { get; set; }

        /// <summary>Tabla de evidencias físicas</summary>
        public DbSet<Evidencia> Evidencias { get; set; }

        /// <summary>
        /// Configura el modelo de datos: nombres de tablas, restricciones, relaciones (FK), índices y valores por defecto.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabla Roles
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(200);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Tabla Usuarios
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);

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

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                // Relación con Roles (1:N) - Restrict: no eliminar rol si tiene usuarios
                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.RolId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.RolId);
                entity.HasIndex(e => e.Activo);
            });

            // Tabla TiposCrimen
            modelBuilder.Entity<TipoCrimen>(entity =>
            {
                entity.ToTable("TiposCrimen");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Nombre);
                entity.HasIndex(e => e.Activo);
            });

            // Tabla ModusOperandi
            modelBuilder.Entity<ModusOperandi>(entity =>
            {
                entity.ToTable("ModusOperandi");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Nombre);
                entity.HasIndex(e => e.Activo);
            });

            // Tabla EscenasCrimen (principal del sistema)
            modelBuilder.Entity<EscenaCrimen>(entity =>
            {
                entity.ToTable("EscenasCrimen");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Ubicacion).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DescripcionDetallada).HasMaxLength(2000);
                entity.Property(e => e.UsuarioRegistro).HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                // Campos booleanos con valor por defecto false
                entity.Property(e => e.UsoViolencia).HasDefaultValue(false);
                entity.Property(e => e.ActoPlanificado).HasDefaultValue(false);
                entity.Property(e => e.MultiplesPerpetadores).HasDefaultValue(false);
                entity.Property(e => e.AutoriaDesconocida).HasDefaultValue(false);

                // Relaciones con catálogos (1:N) - Restrict: no eliminar si hay escenas que los usan
                entity.HasOne(e => e.TipoCrimen)
                    .WithMany(tc => tc.EscenasCrimen)
                    .HasForeignKey(e => e.TipoCrimenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModusOperandi)
                    .WithMany(mo => mo.EscenasCrimen)
                    .HasForeignKey(e => e.ModusOperandiId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización de consultas
                entity.HasIndex(e => e.FechaCrimen);
                entity.HasIndex(e => e.TipoCrimenId);
                entity.HasIndex(e => e.AreaGeografica);
                entity.HasIndex(e => e.ModusOperandiId);
                entity.HasIndex(e => e.FechaRegistro);

                // Índice compuesto para optimizar comparaciones (ComparacionService)
                entity.HasIndex(e => new { e.TipoCrimenId, e.AreaGeografica, e.ModusOperandiId, e.HorarioCrimen })
                    .HasDatabaseName("IX_EscenasCrimen_Comparacion");
            });

            // Tabla Evidencias (dependiente de EscenasCrimen)
            modelBuilder.Entity<Evidencia>(entity =>
            {
                entity.ToTable("Evidencias");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Descripcion).HasMaxLength(500);

                // Relación con EscenaCrimen (1:N) - Cascade: eliminar evidencias al eliminar escena
                entity.HasOne(e => e.EscenaCrimen)
                    .WithMany(ec => ec.Evidencias)
                    .HasForeignKey(e => e.EscenaCrimenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.EscenaCrimenId);
            });
        }
    }
}