using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Application.Common;
using PaintStore.API.Database;
using PaintStore.API.Enums;
using PaintStore.Models;

namespace PaintStore.API.Repositories;

public class PaintProductsRepository
{
    private readonly PaintStoreDbContext _dbContext;
    public PaintProductsRepository(PaintStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<PaintProductResult>> GetKeysetPaginationAsync(int pageSize, 
                                                                        int? lastPageEndId, 
                                                                        CancellationToken cancellationToken)
    {
        IQueryable<PaintProduct> query = _dbContext.PaintProducts;
        if(lastPageEndId != null)
        {
            query = query.Where(p=>p.Id > lastPageEndId);
        }

        List<PaintProductResult> paintProducts = await query.OrderBy(p=>p.Id)
                                                                .Take(pageSize + 1)
                                                                .Select(PaintProductResult.Projection)
                                                                .ToListAsync(cancellationToken);
        return paintProducts;
    }

    public async Task<PaginationOffsetQueryResult<PaintProductResult>> 
        GetOffsetPaginationAsync(int pageSize, int startIndex, CancellationToken cancellationToken)
    {
        await using var transaction = 
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot, 
                                                            cancellationToken);

        List<PaintProductResult> items = await _dbContext.PaintProducts
                                                    .OrderBy(p=>p.Id)
                                                    .Skip(startIndex)
                                                    .Take(pageSize)
                                                    .Select(PaintProductResult.Projection)
                                                    .ToListAsync(cancellationToken);

        int totalCount = await _dbContext.PaintProducts.CountAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PaginationOffsetQueryResult<PaintProductResult> ()
                {
                    Items = items,
                    TotalCount = totalCount
                };
    }

    public async Task<RepositoryResults<int>> DeleteByIdAsync(int id, CancellationToken cancellationToken)
    {
        int affectedRow = 0;
        try
        {
            affectedRow = await _dbContext.PaintProducts.Where(p=>p.Id == id).ExecuteDeleteAsync(cancellationToken);
        }
        catch (SqlException sqlException)
            when(sqlException.Number == 547)
        {
                return new RepositoryResults<int>()
                        {
                            ResultsEnum = RepositoryResultsEnum.ForeignKeyConstraintViolation
                        };
        }
        return new RepositoryResults<int>()
                {
                    ResultsEnum = RepositoryResultsEnum.Success,
                    Data = affectedRow
                };
    }

    public async Task<PaintProductResult?> GetPaintProductResultByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.PaintProducts.Where(p=>p.Id == id)
                                            .Select(PaintProductResult.Projection)
                                            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PaintProduct>> GetPaintProductByIdRangeAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.PaintProducts.Where(p=>ids.Contains(p.Id)).ToListAsync(cancellationToken);
    }

    public async Task<PaintProduct?> GetPaintProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.PaintProducts.Where(p=>p.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CheckeExistedByNameExceptIdAsync(string name, int id, CancellationToken cancellationToken)
    {
        return await _dbContext.PaintProducts.AnyAsync(p=>p.Name == name.Trim() && p.Id != id,
                                                    cancellationToken);
    }

    public async Task<bool> CheckeExistedByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.PaintProducts.AnyAsync(p=>p.Name == name.Trim(), cancellationToken);
    }

    public async Task<bool> CheckeExistedByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.PaintProducts.AnyAsync(p=>p.Id == id, cancellationToken);
    }

    public void SetExpectedRowVersion(PaintProduct paintProduct, byte[] rowVersion)
    {
        _dbContext.Entry(paintProduct).Property(p=>p.RowVersion).OriginalValue = rowVersion;
    }

    public void AddItem(PaintProduct paintProduct)
    {
        _dbContext.PaintProducts.Add(paintProduct);
    }
}
