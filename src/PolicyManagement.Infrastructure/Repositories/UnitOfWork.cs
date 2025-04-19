using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly TenantDbContextBase _dbContext;
    private IDbContextTransaction _transaction;
    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed;

    public UnitOfWork(
        TenantDbContextBase dbContext, 
        IPolicyRepository policyRepository,
        ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _policyRepository = policyRepository;
        _logger = logger;
        _disposed = false;
    }

    public IPolicyRepository PolicyRepository => _policyRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Transaction has not been started. Call BeginTransactionAsync first.");
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            _logger.LogWarning("RollbackTransactionAsync was called but no active transaction exists");
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _dbContext.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            await _dbContext.DisposeAsync();
            _disposed = true;
        }
        
        GC.SuppressFinalize(this);
    }
}