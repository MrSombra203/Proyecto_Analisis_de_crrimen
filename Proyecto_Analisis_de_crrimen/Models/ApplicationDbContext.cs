using Microsoft.EntityFrameworkCore;

namespace Proyecto_Analisis_de_crimen.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoCrimen> TiposCrimen { get; set; }
        public DbSet<ModusOperandi> ModusOperandi { get; set; }
        public DbSet<EscenaCrimen> EscenasCrimen { get; set; }
        public DbSet<Evidencia> Evidencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla Roles
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

                // Índice
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Configuración de la tabla Usuarios
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

                // Relación con Roles
                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.RolId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.RolId);
                entity.HasIndex(e => e.Activo);
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

            // Configuración de la tabla EscenasCrimen
            modelBuilder.Entity<EscenaCrimen>(entity =>
            {
                entity.ToTable("EscenasCrimen");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Ubicacion)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.DescripcionDetallada)
                    .HasMaxLength(2000);

                entity.Property(e => e.UsuarioRegistro)
                    .HasMaxLength(100);

                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("GETDATE()");

                // Valores por defecto para campos booleanos
                entity.Property(e => e.UsoViolencia)
                    .HasDefaultValue(false);

                entity.Property(e => e.ActoPlanificado)
                    .HasDefaultValue(false);

                entity.Property(e => e.MultiplesPerpetadores)
                    .HasDefaultValue(false);

                entity.Property(e => e.AutoriaDesconocida)
                    .HasDefaultValue(false);

                // Relaciones con catálogos
                entity.HasOne(e => e.TipoCrimen)
                    .WithMany(tc => tc.EscenasCrimen)
                    .HasForeignKey(e => e.TipoCrimenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModusOperandi)
                    .WithMany(mo => mo.EscenasCrimen)
                    .HasForeignKey(e => e.ModusOperandiId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para mejorar rendimiento
                entity.HasIndex(e => e.FechaCrimen);
                entity.HasIndex(e => e.TipoCrimenId);
                entity.HasIndex(e => e.AreaGeografica);
                entity.HasIndex(e => e.ModusOperandiId);
                entity.HasIndex(e => e.FechaRegistro);

                // Índice compuesto para búsquedas de comparación
                entity.HasIndex(e => new { e.TipoCrimenId, e.AreaGeografica, e.ModusOperandiId, e.HorarioCrimen })
                    .HasDatabaseName("IX_EscenasCrimen_Comparacion");
            });

            // Configuración de la tabla Evidencias
            modelBuilder.Entity<Evidencia>(entity =>
            {
                entity.ToTable("Evidencias");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                // Relación con EscenaCrimen (CASCADE DELETE)
                entity.HasOne(e => e.EscenaCrimen)
                    .WithMany(ec => ec.Evidencias)
                    .HasForeignKey(e => e.EscenaCrimenId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice en clave foránea
                entity.HasIndex(e => e.EscenaCrimenId);
            });
        }
    }
}