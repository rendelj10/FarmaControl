
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class LoteRepository : ILoteRepository
    {
        private readonly AppDbContext _context;

        public LoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lote>> ObtenerTodasAsync()
        {
            return await _context.Lotes
                .Include(l => l.Medicamento)
                .AsNoTracking()
                .OrderBy(l => l.FechaVencimiento)
                .ThenBy(l => l.NumeroLote)
                .ToListAsync();
        }

        public async Task<Lote?> ObtenerPorIdAsync(int id)
        {
            return await _context.Lotes
                .Include(l => l.Medicamento)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AgregarAsync(Lote lote)
        {
            await _context.Lotes.AddAsync(lote);

            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Lote lote)
        {
            _context.Lotes.Update(lote);

            await _context.SaveChangesAsync();
        }
    }
}
