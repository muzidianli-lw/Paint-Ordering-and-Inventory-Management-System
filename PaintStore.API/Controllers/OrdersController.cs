using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Data;
using PaintStore.Models;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAllOrders()
        {
            return Ok(MockData.Orders);
        }

        [HttpGet("byPrice")]
        public ActionResult GetOrdersByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            if(minPrice < 0)
            {
                return BadRequest("minPrice < 0");
            }
            if (maxPrice <= minPrice)
            {
                return BadRequest("min >= max");
            }

            return Ok(MockData.Orders.Where(o=>o.TotalPrice > minPrice && o.TotalPrice < maxPrice).ToList());
        }

        [HttpGet("byPaint/{paintId:int}")]
        public ActionResult GetOrdersByPaintId([FromRoute] int paintId)
        {
            if (paintId < 0)
            {
                return BadRequest("paintId < 0");
            }
            return Ok(MockData.Orders.Where(o=>o.PaintProducts.Any(p=>p.Id == paintId)).ToList());
        }

        [HttpGet("byUser/{userId:int}")]
        public ActionResult GetOrdersByUserId([FromRoute] int userId)
        {
            if (userId < 0)
            {
                return BadRequest("userId < 0");
            }
            return Ok(MockData.Orders.Where(o=>o.UserId == userId).ToList());
        }

        [HttpGet("lastMonth")]
        public ActionResult GetLastMonthOrders()
        {
            DateTime lastMonthDate = DateTime.Now.AddMonths(-1);
            return Ok(MockData.Orders.Where(o=>o.CreatedDate.Month == lastMonthDate.Month && o.CreatedDate.Year == lastMonthDate.Year).ToList());
        }

        [HttpGet("byDate")]
        public ActionResult GetOrdersByDate([FromQuery] DateTime date)
        {
            return Ok(MockData.Orders.Where(o=>o.CreatedDate.Date == date.Date).ToList());
        }
    }
}
