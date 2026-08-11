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

        [HttpGet]
        public async Task<ActionResult> GetAllOrders(CancellationToken cancellationToken)
        {
            return Ok(await _dbContext.Orders.ToArrayAsync(cancellationToken));
        }

        [HttpGet("byId/{id:int}")]
        public async Task<IActionResult> GetOrderById([FromRoute] int id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                return BadRequest("id < 0");
            }
            Order? order = await _dbContext.Orders
            .Include(o=>o.User)
            .Include(o=>o.PaintProducts)
            .FirstOrDefaultAsync(o=>o.Id == id, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpGet("byPrice")]
        public async Task<ActionResult> GetOrdersByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice, CancellationToken cancellationToken)
        {
            if(minPrice < 0)
            {
                return BadRequest("minPrice < 0");
            }
            if (maxPrice <= minPrice)
            {
                return BadRequest("min >= max");
            }

            List<Order> orders = await _dbContext.Orders.Include(o => o.PaintProducts).ToListAsync(cancellationToken);
            orders = orders.Where(o => o.TotalPrice >= minPrice && o.TotalPrice <= maxPrice).ToList();
            return Ok(orders);
        }

        [HttpGet("byPaint/{paintId:int}")]
        public async Task<ActionResult> GetOrdersByPaintId([FromRoute] int paintId, CancellationToken cancellationToken)
        {
            if (paintId < 0)
            {
                return BadRequest("paintId < 0");
            }

            var query = _dbContext.Orders.Where(o=>o.PaintProducts.Any(p=>p.Id == paintId));
            return Ok(await query.ToListAsync(cancellationToken));
        }

        [HttpGet("byUser/{userId:int}")]
        public async Task<ActionResult> GetOrdersByUserId([FromRoute] int userId, CancellationToken cancellationToken)
        {
            if (userId < 0)
            {
                return BadRequest("userId < 0");
            }
            var query = _dbContext.Orders.Where(o=>o.UserId == userId);
            return Ok(await query.ToListAsync(cancellationToken));
        }

        [HttpGet("lastMonth")]
        public async Task<ActionResult> GetLastMonthOrders(CancellationToken cancellationToken)
        {
            DateTime lastMonthDate = DateTime.Now.AddMonths(-1);
            var query = _dbContext.Orders.Where(o=>o.CreatedAt.Month == lastMonthDate.Month && o.CreatedAt.Year == lastMonthDate.Year);
            return Ok(await query.ToListAsync(cancellationToken));
        }

        [HttpGet("byDate")]
        public async Task<ActionResult> GetOrdersByDate([FromQuery] DateTime date, CancellationToken cancellationToken)
        {
            return Ok(await _dbContext.Orders.Where(o=>o.CreatedAt.Date == date.Date).ToListAsync(cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderDto, CancellationToken cancellationToken)
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
            return CreatedAtAction(nameof(GetOrderById), new {order.Id}, order);            
        }
    }
}
