using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Repositories
{
    /// <summary>
    /// Repositorio específico para EscenaCrimen con operaciones optimizadas
    /// Aplica SRP y OCP (Open/Closed Principle) - Extensible sin modificar código base
    /// </summary>
    public class EscenaCrimenRepository : Repository<EscenaCrimen>, IEscenaCrimenRepository
    {
        public EscenaCrimenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<EscenaCrimen?> GetByIdWithRelationsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EscenaCrimen>> GetAllWithRelationsAsync()
        {
            return await _dbSet
                .Include(e => e.Evidencias)
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<EscenaCrimen>> GetByTipoCrimenAsync(int tipoCrimenId)
        {
            return await _dbSet
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .Where(e => e.TipoCrimenId == tipoCrimenId)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<EscenaCrimen>> GetByAreaGeograficaAsync(AreaGeografica area)
        {
            return await _dbSet
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .Where(e => e.AreaGeografica == area)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<EscenaCrimen>> GetByModusOperandiAsync(int modusOperandiId)
        {
            return await _dbSet
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .Where(e => e.ModusOperandiId == modusOperandiId)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<EscenaCrimen>> GetUltimasEscenasAsync(int cantidad)
        {
            return await _dbSet
                .Include(e => e.TipoCrimen)
                .Include(e => e.ModusOperandi)
                .OrderByDescending(e => e.FechaRegistro)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}



