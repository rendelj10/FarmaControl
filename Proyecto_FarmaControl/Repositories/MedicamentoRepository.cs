
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositories
{
    public class MedicamentoRepository : IMedicamentoRepository
    {
        private readonly AppDbContext _context;

        public MedicamentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medicamento>> ObtenerTodasAsync()
        {
            return await _context.Medicamentos
                .Include(m => m.Categoria)
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Medicamento?> ObtenerPorIdAsync(int id)
        {
            return await _context.Medicamentos
                .Include(m => m.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AgregarAsync(Medicamento medicamento)
        {
            await _context.Medicamentos.AddAsync(medicamento);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Medicamento medicamento)
        {
            _context.Medicamentos.Update(medicamento);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteCodigoAsync(
            string codigo, int? excluirId = null)
        {
            return await _context.Medicamentos.AnyAsync(m =>
                m.Codigo == codigo &&
                (!excluirId.HasValue || m.Id != excluirId.Value));
        }
    }
}
