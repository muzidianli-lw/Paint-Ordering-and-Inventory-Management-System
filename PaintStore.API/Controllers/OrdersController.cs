using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
using PaintStore.Models;

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

        /*
        [HttpGet]
        public async Task<ActionResult> GetAllOrders(CancellationToken cancellationToken)
        {
            return Ok(await _dbContext.Orders.ToArrayAsync(cancellationToken));
        }
        */

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetOrderById([FromRoute] int id,
                                                        CancellationToken cancellationToken)
        {
            OrderResponseDto? order = await _dbContext.Orders
                                        .Include(o=>o.User)
                                        .Include(o=>o.PaintProducts)
                                        .Select(o=>new OrderResponseDto()
                                            {
                                                Id = o.Id,
                                                UserId = o.UserId,
                                                User = o.User,
                                                PaintProducts = o.PaintProducts,
                                                TotalPrice = o.TotalPrice,
                                                RowVersion = o.RowVersion
                                            })
                                        .FirstOrDefaultAsync(o=>o.Id == id, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpGet("byUser/{id:int:min(1)}")]
        public async Task<ActionResult> GetOrdersByUserId([FromRoute] int id,
                                                            CancellationToken cancellationToken)
        {
            return Ok(await _dbContext.Orders.Where(o=>o.UserId == id)
                                            .Include(o=>o.User)
                                            .Include(o=>o.PaintProducts)
                                            .Select(o=>new OrderResponseDto()
                                                {
                                                    Id = o.Id,
                                                    UserId = o.UserId,
                                                    User = o.User,
                                                    PaintProducts = o.PaintProducts,
                                                    TotalPrice = o.TotalPrice,
                                                    RowVersion = o.RowVersion
                                                })
                                            .ToListAsync(cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderDto,
                                                    CancellationToken cancellationToken)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == orderDto.UserId, cancellationToken);
            if (user == null)
            {
                return BadRequest("user id not found");
            }

            if (orderDto.PaintProductIds.Count <= 0)
            {
                return BadRequest("order must have paintproducts");
            }
            List<PaintProduct> paintProducts = [];
            foreach(var productId in orderDto.PaintProductIds)
            {
                PaintProduct? paintProduct = await _dbContext.PaintProducts.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
                if (paintProduct == null)
                {
                    return BadRequest("paintproduct id is not found");
                }
                paintProducts.Add(paintProduct);
            }

            Order order = new Order(orderDto.UserId, user, paintProducts);

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(nameof(GetOrderById), new {order.Id}, new OrderResponseDto(order));            
        }
    }
}
