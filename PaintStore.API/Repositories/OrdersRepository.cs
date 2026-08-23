using System.Data;
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

    public async Task<bool> CheckOrderExistedByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<OrderResult?> GetOrderResultByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.Select(OrderResult.Projection)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(o=>o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetDetailedOrderByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.Include(o=>o.User)
                                    .Include(o=>o.OrderItems)
                                    .FirstOrDefaultAsync(o=>o.Id == orderId, cancellationToken);
    }

    public void AddOrder(Order order)
    {
        _dbContext.Orders.Add(order);
    }

    public void SetExpectedOriginalValue(Order order, byte[] rowVersion)
    {
        _dbContext.Entry(order).Property(o=>o.RowVersion).OriginalValue = rowVersion;
    }

    public async Task<PaginationOffsetQueryResult<OrderResult>> 
        GetPaginationOffsetInfoAsync(int offset, int pageSize, int? userId, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _dbContext.Orders;
        int orderCnt = 0;

        await using var transaction = 
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        if (userId != null)
        {
            query = query.Where(o=>o.UserId == userId);
            orderCnt = await _dbContext.Orders.CountAsync(o=>o.UserId == userId, cancellationToken);
        }
        else
        {
            orderCnt = await _dbContext.Orders.CountAsync(cancellationToken);
        }

        List<OrderResult> orders = await query.OrderBy(o=>o.Id)
                                                .Skip(offset)
                                                .Take(pageSize)
                                                .AsNoTracking()
                                                .Select(OrderResult.Projection)
                                                .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PaginationOffsetQueryResult<OrderResult>(){
                                    Items=orders,
                                    TotalCount = orderCnt};
    }

    public async Task<List<OrderResult>> GetNextPageAsync(int? lastPageEndId, int pageSize, CancellationToken cancellationToken)
    {
        
        IQueryable<Order> query = _dbContext.Orders.OrderBy(o=>o.Id);
        
        if (lastPageEndId != null)
        {
            query = query.Where(o=>o.Id > lastPageEndId);
        }

        return await query.Take(pageSize + 1)
                            .AsNoTracking()
                            .Select(OrderResult.Projection)
                            .ToListAsync(cancellationToken);
    }
}