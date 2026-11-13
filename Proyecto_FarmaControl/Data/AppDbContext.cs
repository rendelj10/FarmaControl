
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
    }
}
