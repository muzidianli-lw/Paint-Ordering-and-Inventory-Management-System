using System;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;

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
}
