
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ObtenerTodasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AgregarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteNombreAsync(
            string nombre, int? excluirId = null)
        {
            return await _context.Categorias.AnyAsync(c =>
                c.Nombre == nombre &&
                (!excluirId.HasValue || c.Id != excluirId.Value));
        }
    }
}
