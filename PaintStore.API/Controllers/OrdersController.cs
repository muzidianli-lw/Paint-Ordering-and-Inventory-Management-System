using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Application.Common;
using PaintStore.API.Application.Orders;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Services;
using PaintStore.Models;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly PaintStoreDbContext _dbContext;
        private readonly OrdersService _ordersService;

        public OrdersController(PaintStoreDbContext dbContext, OrdersService ordersService)
        {
            _dbContext = dbContext;
            _ordersService = ordersService;
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
            ServiceResultsEnum response =  await _ordersService.DeleteOrderAsync(id, cancellationToken);
            return response switch
            {
                ServiceResultsEnum.Success => NoContent(),
                ServiceResultsEnum.NotExisted => NotFound(),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrder([FromRoute] int id,
                                                    [FromBody] OrderUpdateRequestDto request,
                                                    CancellationToken cancellationToken)
        {
            Order? order = await _dbContext.Orders
                                            .Include(o=>o.User)
                                            .Include(o=>o.OrderItems)
                                            .FirstOrDefaultAsync(o=>o.Id == id, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }

            Dictionary<int, int> oldIdToCnt = order.OrderItems.ToDictionary(o=>o.PaintProductId, o=>o.Quantity);
            Dictionary<int, int> newIdToCnt = request.OrderItems
                                                    .GroupBy(i=>i.PaintProductId)
                                                    .ToDictionary(g=>g.Key, g=>-g.Sum(i=>i.Quantity));
            Dictionary<int, int> delta = oldIdToCnt.Concat(newIdToCnt)
                                                .GroupBy(i=>i.Key)
                                                .ToDictionary(g=>g.Key, g=>g.Sum(i=>i.Value));

            List<PaintProduct> PaintProducts= await _dbContext.PaintProducts
                                                .Where(p=>newIdToCnt.Keys.Contains(p.Id) ||
                                                            oldIdToCnt.Keys.Contains(p.Id))
                                                .ToListAsync(cancellationToken);

            if (PaintProducts.Count != delta.Keys.Count)
            {
                return Conflict("some paint product not existed");
            }

            bool notEnough = PaintProducts.Any(p=>p.Inventory + delta[p.Id]<0);
            if (notEnough)
            {
                return Conflict("some paint product not enough");
            }

            foreach (var p in PaintProducts)
            {
                p.UpdateInventory(delta[p.Id]);
            }

            List<OrderItem> items = PaintProducts
                            .Where(p=>newIdToCnt.Keys.Contains(p.Id))
                            .Select(p=>new OrderItem(){PaintProductId = p.Id,
                                                    Quantity = -newIdToCnt[p.Id],
                                                    PaintProduct = p,
                                                    UnitPrice = p.Price})
                            .ToList();       

            order.Update(items);

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

        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderByOrderId([FromRoute] int id,
                                                        CancellationToken cancellationToken)
        {
            ServiceResult<OrderResult> response = 
                await _ordersService.GetOrderByOrderIdAsync(id, cancellationToken);

            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.Success => Ok(response.Data),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpGet("byUser/{id:int:min(1)}")]
        public async Task<ActionResult<PaginationOffsetResponseDto<OrderResponseDto>>> 
            GetOrdersByUserId(
                [FromRoute] int id,
                [FromQuery] PaginationOffsetRequestDto request,
                CancellationToken cancellationToken)
        {
            bool userExisted = await _dbContext.Users.AnyAsync(u=>u.Id == id, cancellationToken);
            if(!userExisted)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto request,
                                                    CancellationToken cancellationToken)
        {
            ServiceResult<OrderResult> response =  await _ordersService.CreateOrderAsync(request.UserId,
                                                                                        request.Items,
                                                                                        cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.NotEnough => Conflict(response.ErrorMsg),
                ServiceResultsEnum.Success => 
                CreatedAtAction(nameof(GetOrderByOrderId), new {response.Data!.Id}, response.Data),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
