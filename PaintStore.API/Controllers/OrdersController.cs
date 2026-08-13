using System.Threading.Tasks;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
using PaintStore.Models;
using System.Collections.Immutable;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly PaintStoreDbContext _dbContext;

        public OrdersController(PaintStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("page")]
        public async Task<ActionResult<PaginationOffsetResponseDto<OrderResponseDto>>>
            GetSpecificPage([FromQuery] PaginationOffsetRequestDto request,
                            CancellationToken cancellationToken)
        {
            long offset = ((long)request.Page - 1) * request.PageSize;
            if(offset > int.MaxValue)
            {
                return BadRequest("input page too large");
            }

            await using var transaction = await _dbContext.Database
                                                .BeginTransactionAsync(IsolationLevel.Snapshot,
                                                                        cancellationToken);

            List<OrderResponseDto> orders = await _dbContext.Orders
                                                            .OrderBy(o=>o.Id)
                                                            .Skip((int)offset)
                                                            .Take(request.PageSize)
                                                            .AsNoTracking()
                                                            .Select(OrderResponseDto.Projection)
                                                            .ToListAsync(cancellationToken);
            int orderCnt = await _dbContext.Orders.CountAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new PaginationOffsetResponseDto<OrderResponseDto>()
                                                        {
                                                            Items=orders,
                                                            TotalCount=orderCnt,
                                                            Page=request.Page,
                                                            PageSize=request.PageSize
                                                        });
        }

        [HttpGet]
        public async Task<ActionResult<PaginationKeysetResponseDto<OrderResponseDto>>> 
            GetNextPage([FromQuery] PaginationKeysetRequestDto request,
                        CancellationToken cancellationToken)
        {
            
            IQueryable<Order> query = _dbContext.Orders.OrderBy(o=>o.Id);
            
            if (request.LastPageEndId is int lastPageEndId)
            {
                query = query.Where(o=>o.Id > lastPageEndId);
            }

            List<OrderResponseDto> orders = await query.Take(request.PageSize + 1)
                                                        .AsNoTracking()
                                                        .Select(OrderResponseDto.Projection)
                                                        .ToListAsync(cancellationToken);

            bool hasNextPage = orders.Count > request.PageSize;
            if(hasNextPage)
            {
                orders.RemoveAt(request.PageSize);
            }
            int? thisPageEndId = orders.Count > 0 ? orders[^1].Id : null;

            return Ok(new PaginationKeysetResponseDto<OrderResponseDto> ()
                {
                    Items=orders,
                    HasNextPage=hasNextPage,
                    ThisPageEndId=thisPageEndId
                });
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<ActionResult> DeleteOrder([FromRoute] int id,
                                                    CancellationToken cancellationToken)
        {
            int affectedRow = await _dbContext.Orders.Where(o=>o.Id == id).ExecuteDeleteAsync(cancellationToken);
            if(affectedRow == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
/*
        [HttpPut("{id:int:min(1)}")]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrder([FromRoute] int id,
                                                    [FromBody] OrderUpdateRequestDto request,
                                                    CancellationToken cancellationToken)
        {
            Order? order = await _dbContext.Orders
                                            .Include(o=>o.User)
                                            .Include(o=>o.PaintProducts)
                                            .FirstOrDefaultAsync(o=>o.Id == id, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }

            List<PaintProduct> paintProducts = await _dbContext.PaintProducts
                                                        .Where(p=>request.PaintProductIds.Contains(p.Id))
                                                        .ToListAsync(cancellationToken);
            order.Update(paintProducts);

            _dbContext.Entry(order)
                    .Property(o=>o.RowVersion)
                    .OriginalValue = request.RowVersion;
            
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                bool existed = await _dbContext.Orders.AnyAsync(o => o.Id == id, cancellationToken);
                if(existed)
                {
                    return Conflict("data changed");
                }
                return NotFound();
            }
            catch (DbUpdateException exception)
                when(exception.InnerException is SqlException sqlException
                    && sqlException.Number == 547)
            {
                return Conflict("related paintproduct is deleted");
            }
            return Ok(OrderResponseDto.FromEntity(order));
        }
*/
        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById([FromRoute] int id,
                                                        CancellationToken cancellationToken)
        {
            OrderResponseDto? order = await _dbContext.Orders
                                        .Select(OrderResponseDto.Projection)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(o=>o.Id == id, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpGet("byUser/{id:int:min(1)}")]
        public async Task<ActionResult<PaginationOffsetResponseDto<OrderResponseDto>>> 
            GetOrdersByUserId(
                [FromRoute] int id,
                [FromQuery] PaginationOffsetRequestDto request,
                CancellationToken cancellationToken)
        {
            long offset = ((long)request.Page - 1) * request.PageSize;
            if(offset > int.MaxValue)
            {
                return BadRequest("input page too large");
            }

            await using var transaction = 
                await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

            List<OrderResponseDto> orders = await _dbContext.Orders.Where(o=>o.UserId == id)
                                                                    .OrderBy(o=>o.Id)
                                                                    .Skip((int)offset)
                                                                    .Take(request.PageSize)
                                                                    .AsNoTracking()
                                                                    .Select(OrderResponseDto.Projection)
                                                                    .ToListAsync(cancellationToken);
            int orderCnt = await _dbContext.Orders.CountAsync(o=>o.UserId == id, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new PaginationOffsetResponseDto<OrderResponseDto>()
                                                        {
                                                            Items=orders,
                                                            TotalCount=orderCnt,
                                                            Page=request.Page,
                                                            PageSize=request.PageSize
                                                        });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderDto,
                                                    CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users
                            .FirstOrDefaultAsync(u=>u.Id == orderDto.UserId, cancellationToken);
            if (user == null)
            {
                return BadRequest("user id not found");
            }

            Dictionary<int, int> newItemIdCnt = orderDto.Items
                                            .GroupBy(i=>i.PaintProductId)
                                            .ToDictionary(g=>g.Key, g=>g.Sum(i=>i.Quantity));

            List<int> newItemIds = newItemIdCnt.Select(i=>i.Key).ToList();

            List<PaintProduct> paintProducts= await _dbContext.PaintProducts
                                                .Where(p=>newItemIds.Contains(p.Id))
                                                .ToListAsync(cancellationToken);

            if (paintProducts.Count != newItemIds.Count)
            {
                return Conflict("some paint product not existed");
            }

            bool notEnough = paintProducts.Any(p=>p.Inventory < newItemIdCnt[p.Id]);
            if (notEnough)
            {
                return Conflict("some paint product inventory is not enough");
            }

            foreach(var paintProduct in paintProducts)
            {
                paintProduct.ReduceInventory(newItemIdCnt[paintProduct.Id]);
            }

            Dictionary<int, PaintProduct> IdPaintProduct = 
                                        paintProducts.ToDictionary(p=>p.Id, p=>p);

            List<OrderItem> orderItems = paintProducts
                                        .Select(p=>new OrderItem()
                                        {
                                            PaintProductId = p.Id,
                                            Quantity = newItemIdCnt[p.Id],
                                            PaintProduct = p,
                                            UnitPrice = p.Price
                                        })
                                        .ToList();
            Order order = new Order(orderDto.UserId, user, orderItems);

            _dbContext.Orders.Add(order);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("related data is deleted or changed");
            }
            catch (DbUpdateException exception)
                when(exception.InnerException is SqlException sqlException
                    && sqlException.Number == 547)
            {
                return Conflict("related data is deleted");
            }

            return CreatedAtAction(nameof(GetOrderById), new {order.Id}, OrderResponseDto.FromEntity(order));            
        }
    }
}
