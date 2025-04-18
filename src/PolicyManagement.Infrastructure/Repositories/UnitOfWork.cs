using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PolicyManagement.Application.Interfaces.Repositories;

namespace PolicyManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    private IDbContextTransaction _transaction;
    
    private IPolicyRepository _policyRepository;

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IPolicyRepository PolicyRepository => _policyRepository ??= new PolicyRepository(_dbContext);

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
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
} 