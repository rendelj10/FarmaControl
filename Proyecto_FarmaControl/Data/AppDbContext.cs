
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
            = null!;

        public DbSet<Medicamento> Medicamentos { get; set; }
            = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Medicamento>(entity =>
            {
                entity.HasIndex(m => m.Codigo)
                    .IsUnique();

                entity.HasOne(m => m.Categoria)
                    .WithMany()
                    .HasForeignKey(m => m.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
