using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Application.Common;
using PaintStore.API.Application.Orders;
using PaintStore.API.Database;
using PaintStore.API.Enums;
using PaintStore.Models;

namespace PaintStore.API.Repositories;

public class OrdersRepository
{
    private readonly PaintStoreDbContext _dbContext;

    public OrdersRepository(PaintStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> DeleteOrderByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.Where(o=>o.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<OrderResult?> GetOrderByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.Select(OrderResult.Projection)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(o=>o.Id == orderId, cancellationToken);
    }

    public void AddOrder(Order order)
    {
        _dbContext.Orders.Add(order);
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
            switch (sqlException.Number)
            {
                case 547: return RepositoryResultsEnum.ForeignKeyConstraintViolation;
                default: throw;
            }
        }
        return RepositoryResultsEnum.Success;
    }
}
