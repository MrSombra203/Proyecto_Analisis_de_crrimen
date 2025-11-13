using Microsoft.EntityFrameworkCore;

namespace Proyecto_Analisis_de_crimen.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<EscenaCrimen> EscenasCrimen { get; set; }
        public DbSet<Evidencia> Evidencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

                // Índices para mejorar rendimiento
                entity.HasIndex(e => e.FechaCrimen);
                entity.HasIndex(e => e.TipoCrimen);
                entity.HasIndex(e => e.AreaGeografica);
                entity.HasIndex(e => e.FechaRegistro);
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