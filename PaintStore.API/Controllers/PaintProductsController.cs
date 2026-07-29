using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.Models;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaintProductsController : ControllerBase
    {
        private readonly PaintStoreDbContext _dbContext;
        public PaintProductsController(PaintStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPaintProducts(CancellationToken cancellationToken)
        {
            return Ok(await _dbContext.PaintProducts.ToListAsync(cancellationToken));
        }

        [HttpGet("by-price")]
        public async Task<ActionResult> GetProductsByPriceRange([FromQuery] decimal min, [FromQuery] decimal max, CancellationToken cancellationToken)
        {
            if (min < 0)
            {
                return BadRequest("min < 0");
            }
            if (max <= min)
            {
                return BadRequest("max <= min");
            }
            var query = _dbContext.PaintProducts.Where(p=>p.Price < max && p.Price > min);
            return Ok(await query.ToListAsync(cancellationToken));
        }

        [HttpGet("by-paint-id/{paintId:int}")]
        public async Task<IActionResult> GetPaintProductsByPaintId([FromRoute] int paintId, CancellationToken cancellationToken)
        {
            if (paintId < 0)
            {
                return BadRequest("paintId < 0");
            }

            PaintProduct? paintProduct = await _dbContext.PaintProducts.FirstOrDefaultAsync(p=>p.Id == paintId, cancellationToken);
            if (paintProduct == null)
            {
                return NotFound();
            }

            return Ok(paintProduct);
        }

        [HttpGet("by-user-id/{userId:int}")]
        public async Task<ActionResult> GetPaintProductsByUserId([FromRoute] int userId, CancellationToken cancellationToken)
        {
            if (userId < 0)
            {
                return BadRequest("userId < 0");
            }
            var query = _dbContext.Orders.Where(o=>o.UserId == userId).SelectMany(o=>o.PaintProducts);
            List<PaintProduct> paintProducts = await query.ToListAsync(cancellationToken);
            return Ok(paintProducts.DistinctBy(p=>p.Id).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreatPaintProduct([FromBody] PaintProduct paintProduct, CancellationToken cancellationToken)
        {
            PaintProduct paintProductUsed = new PaintProduct(paintProduct.Name, paintProduct.Price);
            _dbContext.PaintProducts.Add(paintProductUsed);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(nameof(GetPaintProductsByPaintId), new {paintId=paintProductUsed.Id}, paintProductUsed);
        }
    }
}
