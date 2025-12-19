using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Repositories;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio de gestión de escenas de crimen (SRP - Single Responsibility Principle)
    /// Separa la lógica de negocio de los controladores
    /// </summary>
    public class EscenaCrimenService : IEscenaCrimenService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EscenaCrimenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<EscenaCrimen>> ObtenerTodasAsync()
        {
            return await _unitOfWork.EscenasCrimen.GetAllWithRelationsAsync();
        }

        public async Task<EscenaCrimen?> ObtenerPorIdAsync(int id)
        {
            return await _unitOfWork.EscenasCrimen.GetByIdWithRelationsAsync(id);
        }

        public async Task<EscenaCrimen> RegistrarEscenaAsync(EscenaCrimen escena, string usuarioRegistro)
        {
            // Preparar datos
            escena.FechaRegistro = DateTime.Now;
            escena.UsuarioRegistro = usuarioRegistro;

            // Guardar escena
            await _unitOfWork.EscenasCrimen.AddAsync(escena);
            
            // Guardar evidencias si existen
            if (escena.Evidencias != null && escena.Evidencias.Any())
            {
                await _unitOfWork.Evidencias.AddRangeAsync(escena.Evidencias);
            }

            await _unitOfWork.SaveChangesAsync();
            return escena;
        }

        public async Task<bool> ValidarEscenaAsync(EscenaCrimen escena)
        {
            // Validar ubicación
            if (string.IsNullOrWhiteSpace(escena.Ubicacion))
                return false;

            // Validar TipoCrimen existe y está activo
            if (escena.TipoCrimenId > 0)
            {
                var tipoCrimen = await _unitOfWork.TiposCrimen.GetByIdAsync(escena.TipoCrimenId);
                if (tipoCrimen == null || !tipoCrimen.Activo)
                    return false;
            }
            else
            {
                return false;
            }

            // Validar ModusOperandi existe y está activo
            if (escena.ModusOperandiId > 0)
            {
                var modusOperandi = await _unitOfWork.ModusOperandi.GetByIdAsync(escena.ModusOperandiId);
                if (modusOperandi == null || !modusOperandi.Activo)
                    return false;
            }
            else
            {
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<TipoCrimen>> ObtenerTiposCrimenActivosAsync()
        {
            return await _unitOfWork.TiposCrimen.FindAsync(t => t.Activo);
        }

        public async Task<IEnumerable<ModusOperandi>> ObtenerModusOperandiActivosAsync()
        {
            return await _unitOfWork.ModusOperandi.FindAsync(m => m.Activo);
        }

        public async Task<int> ObtenerTotalEscenasAsync()
        {
            return await _unitOfWork.EscenasCrimen.CountAsync();
        }

        public async Task<IEnumerable<EscenaCrimen>> ObtenerUltimasEscenasAsync(int cantidad)
        {
            return await _unitOfWork.EscenasCrimen.GetUltimasEscenasAsync(cantidad);
        }
    }
}



