using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.Enums;

namespace PaintStore.API.Repositories;

public class UnitOfWork
{
    private readonly PaintStoreDbContext _dbContext;

    public UnitOfWork(PaintStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RepositoryResultsEnum> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return RepositoryResultsEnum.ConcurrencyException;
        }
        catch (DbUpdateException exception)
            when(exception.InnerException is SqlException sqlException)
        {
            switch(sqlException.Number)
            {
                case 2601: return RepositoryResultsEnum.UniqueIndexDuplicated;
                case 547: return RepositoryResultsEnum.ForeignKeyConstraintViolation;
                default: throw;
            };
        }
        return RepositoryResultsEnum.Success;
    }
}
