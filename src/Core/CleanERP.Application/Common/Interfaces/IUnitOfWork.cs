namespace CleanERP.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions and coordinating multiple repository operations
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save all changes made in this unit of work
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if there is an active transaction
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Execute a function within a transaction scope
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the function</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action within a transaction scope
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
