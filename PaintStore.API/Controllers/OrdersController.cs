using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
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
            var query = _dbContext.Orders.Where(o=>o.TotalPrice > minPrice && o.TotalPrice < maxPrice);

            return Ok(await query.ToListAsync(cancellationToken));
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
        public IActionResult CreateOrder([FromBody] Order order)
        {
            // todo
            return Ok();            
        }
    }
}
