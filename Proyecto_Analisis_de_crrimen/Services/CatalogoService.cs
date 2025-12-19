using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Repositories;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio de gestión de catálogos (SRP)
    /// Maneja la lógica de negocio para TiposCrimen y ModusOperandi
    /// </summary>
    public class CatalogoService : ICatalogoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CatalogoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Tipos de Crimen

        public async Task<IEnumerable<TipoCrimen>> ObtenerTiposCrimenAsync()
        {
            return await _unitOfWork.TiposCrimen.GetAllAsync();
        }

        public async Task<TipoCrimen?> ObtenerTipoCrimenPorIdAsync(int id)
        {
            return await _unitOfWork.TiposCrimen.GetByIdAsync(id);
        }

        public async Task<TipoCrimen> CrearTipoCrimenAsync(TipoCrimen tipoCrimen)
        {
            tipoCrimen.FechaCreacion = DateTime.Now;
            tipoCrimen.Activo = true;

            await _unitOfWork.TiposCrimen.AddAsync(tipoCrimen);
            await _unitOfWork.SaveChangesAsync();
            return tipoCrimen;
        }

        public async Task<TipoCrimen> ActualizarTipoCrimenAsync(TipoCrimen tipoCrimen)
        {
            _unitOfWork.TiposCrimen.Update(tipoCrimen);
            await _unitOfWork.SaveChangesAsync();
            return tipoCrimen;
        }

        public async Task<bool> CambiarEstadoTipoCrimenAsync(int id)
        {
            var tipoCrimen = await _unitOfWork.TiposCrimen.GetByIdAsync(id);
            if (tipoCrimen == null)
                return false;

            tipoCrimen.Activo = !tipoCrimen.Activo;
            _unitOfWork.TiposCrimen.Update(tipoCrimen);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TipoCrimenExisteAsync(string nombre, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.TiposCrimen.AnyAsync(t => t.Nombre == nombre && t.Id != excludeId.Value);
            }
            return await _unitOfWork.TiposCrimen.AnyAsync(t => t.Nombre == nombre);
        }

        #endregion

        #region Modus Operandi

        public async Task<IEnumerable<ModusOperandi>> ObtenerModusOperandiAsync()
        {
            return await _unitOfWork.ModusOperandi.GetAllAsync();
        }

        public async Task<ModusOperandi?> ObtenerModusOperandiPorIdAsync(int id)
        {
            return await _unitOfWork.ModusOperandi.GetByIdAsync(id);
        }

        public async Task<ModusOperandi> CrearModusOperandiAsync(ModusOperandi modusOperandi)
        {
            modusOperandi.FechaCreacion = DateTime.Now;
            modusOperandi.Activo = true;

            await _unitOfWork.ModusOperandi.AddAsync(modusOperandi);
            await _unitOfWork.SaveChangesAsync();
            return modusOperandi;
        }

        public async Task<ModusOperandi> ActualizarModusOperandiAsync(ModusOperandi modusOperandi)
        {
            _unitOfWork.ModusOperandi.Update(modusOperandi);
            await _unitOfWork.SaveChangesAsync();
            return modusOperandi;
        }

        public async Task<bool> CambiarEstadoModusOperandiAsync(int id)
        {
            var modusOperandi = await _unitOfWork.ModusOperandi.GetByIdAsync(id);
            if (modusOperandi == null)
                return false;

            modusOperandi.Activo = !modusOperandi.Activo;
            _unitOfWork.ModusOperandi.Update(modusOperandi);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ModusOperandiExisteAsync(string nombre, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.ModusOperandi.AnyAsync(m => m.Nombre == nombre && m.Id != excludeId.Value);
            }
            return await _unitOfWork.ModusOperandi.AnyAsync(m => m.Nombre == nombre);
        }

        #endregion
    }
}



