using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Data;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaintProductsController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAllPaintProducts()
        {
            return Ok(MockData.Orders.SelectMany(o=>o.PaintProducts).ToList());
        }

        [HttpGet("by-price")]
        public ActionResult GetProductsByPriceRange([FromQuery] decimal min, [FromQuery] decimal max)
        {
            if (min < 0)
            {
                return BadRequest("min < 0");
            }
            if (max <= min)
            {
                return BadRequest("max <= min");
            }
            return Ok(MockData.Orders.SelectMany(o=>o.PaintProducts).Where(p=>p.Price < max && p.Price > min).ToList());
        }

        [HttpGet("by-paint-id/{paintId:int}")]
        public ActionResult GetPaintProductsByPaintId([FromRoute] int paintId)
        {
            if (paintId < 0)
            {
                return BadRequest("paintId < 0");
            }
            return Ok(MockData.Orders.SelectMany(o=>o.PaintProducts).DistinctBy(p=>p.Id).Where(p=>p.Id == paintId).ToList()[0]);
        }

        [HttpGet("by-user-id/{userId:int}")]
        public ActionResult GetPaintProductsByUserId([FromRoute] int userId)
        {
            if (userId < 0)
            {
                return BadRequest("userId < 0");
            }
            return Ok(MockData.Orders.Where(o=>o.UserId == userId).SelectMany(o=>o.PaintProducts).ToList());
        }
    }
}
