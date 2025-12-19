using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Repositories
{
    /// <summary>
    /// Implementación de Unit of Work Pattern
    /// Coordina múltiples repositorios y maneja transacciones
    /// Aplica DIP y SRP
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IEscenaCrimenRepository? _escenasCrimen;
        private IRepository<Evidencia>? _evidencias;
        private IRepository<TipoCrimen>? _tiposCrimen;
        private IRepository<ModusOperandi>? _modusOperandi;
        private IRepository<Usuario>? _usuarios;
        private IRepository<Rol>? _roles;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEscenaCrimenRepository EscenasCrimen
        {
            get
            {
                _escenasCrimen ??= new EscenaCrimenRepository(_context);
                return _escenasCrimen;
            }
        }

        public IRepository<Evidencia> Evidencias
        {
            get
            {
                _evidencias ??= new Repository<Evidencia>(_context);
                return _evidencias;
            }
        }

        public IRepository<TipoCrimen> TiposCrimen
        {
            get
            {
                _tiposCrimen ??= new Repository<TipoCrimen>(_context);
                return _tiposCrimen;
            }
        }

        public IRepository<ModusOperandi> ModusOperandi
        {
            get
            {
                _modusOperandi ??= new Repository<ModusOperandi>(_context);
                return _modusOperandi;
            }
        }

        public IRepository<Usuario> Usuarios
        {
            get
            {
                _usuarios ??= new Repository<Usuario>(_context);
                return _usuarios;
            }
        }

        public IRepository<Rol> Roles
        {
            get
            {
                _roles ??= new Repository<Rol>(_context);
                return _roles;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}



