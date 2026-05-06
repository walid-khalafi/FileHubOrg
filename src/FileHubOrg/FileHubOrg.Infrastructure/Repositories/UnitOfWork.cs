using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
    {
        private readonly FileHubOrgDbContext _context;

        // Current transaction reference (null if no active transaction)
        private IDbContextTransaction? _currentTransaction;     
        public IApplicationUserRepository ApplicationUsers {get;}

        public IApplicationRoleRepository ApplicationRoles { get; }

        public UnitOfWork(FileHubOrgDbContext context,IApplicationUserRepository applicationUsers, IApplicationRoleRepository applicationRoles)
        {
            _context = context;
            ApplicationUsers = applicationUsers;
            ApplicationRoles = applicationRoles;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is not null) return;
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
                throw new InvalidOperationException("No active transaction to commit. Call BeginTransactionAsync() first.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Synchronous dispose (for frameworks/tests expecting IDisposable).
        /// Internally calls async dispose.
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronous dispose.
        /// Cleans up transaction and DbContext.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }

            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execute a unit of work inside a transaction.
        /// Rolls back automatically if the action throws.
        /// </summary>
        public async Task ExecuteInTransactionAsync(
           Func<CancellationToken, Task> action,
           CancellationToken cancellationToken = default)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));

            await BeginTransactionAsync(cancellationToken);
            try
            {
                await action(cancellationToken);
                await CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Generic repository accessor for any entity type.
        /// </summary>
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
            => new GenericRepository<TEntity>(_context);


        /// <summary>
        /// Roll back the current transaction.
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
                throw new InvalidOperationException("No active transaction to roll back.");

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Persist all pending changes atomically.
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
