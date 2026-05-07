using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHubOrg.Domain.Interfaces
{

    /// <summary>
    /// Defines the Unit of Work pattern for coordinating repositories.
    /// Ensures all repositories share the same persistence context (DbContext)
    /// and provides transactional boundaries for multi-aggregate operations.
    /// 
    /// Purpose:
    /// - Centralize repository access
    /// - Guarantee atomic persistence of changes
    /// - Provide transaction control (begin, commit, rollback)
    /// - Keep EF Core details out of the Domain layer
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        // -----------------------------
        // Repository accessors
        // -----------------------------

        /// <summary>
        /// Repository for application users (Identity).
        /// </summary>
        public IApplicationUserRepository ApplicationUsers { get; }

        /// <summary>
        /// Repository for application roles (Identity).
        /// </summary>
        public IApplicationRoleRepository ApplicationRoles { get; }

        /// <summary>
        /// Repository for organization's department
        /// </summary>
        public IDepartmentRepository Departments { get; }

        public IJWTRepository JWTs { get; }

        public IFileMemberRepository FileMembers { get; }
        public IFileMetaDataRepository FileMetaData { get; }
        // -----------------------------
        // Generic repository access
        // -----------------------------

        /// <summary>
        /// Optional generic repository accessor.
        /// Provides CRUD operations for any entity type.
        /// Implemented in Infrastructure if GenericRepository is available.
        /// </summary>
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;


        // -----------------------------
        // Persistence and transactions
        // -----------------------------

        /// <summary>
        /// Persists all pending changes atomically.
        /// Ensures consistency across all repositories.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Convenience helper to execute a unit of work inside a transaction.
        /// Rolls back automatically if an exception occurs, then rethrows.
        /// </summary>
        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default);

    }
}
