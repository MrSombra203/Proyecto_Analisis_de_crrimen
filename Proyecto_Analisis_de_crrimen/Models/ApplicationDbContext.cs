using System.Collections.Generic;

nusing System.Data.Entity;

namespace Proyecto_Analisis_de_crimen.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<EscenaCrimen> EscenasCrimen { get; set; }
        public DbSet<Evidencia> Evidencias { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}