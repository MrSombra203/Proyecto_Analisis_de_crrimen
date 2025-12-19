using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Repositories
{
    /// <summary>
    /// Unit of Work Pattern - Coordina múltiples repositorios y transacciones
    /// Aplica DIP (Dependency Inversion Principle)
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IEscenaCrimenRepository EscenasCrimen { get; }
        IRepository<Evidencia> Evidencias { get; }
        IRepository<TipoCrimen> TiposCrimen { get; }
        IRepository<ModusOperandi> ModusOperandi { get; }
        IRepository<Usuario> Usuarios { get; }
        IRepository<Rol> Roles { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

