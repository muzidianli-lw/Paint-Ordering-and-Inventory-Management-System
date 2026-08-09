using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
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

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetPaintProduct([FromRoute] int id, CancellationToken cancellationToken)
        {
            PaintProductResponseDto? response = await _dbContext.PaintProducts.Where(p=>p.Id == id)
                                                                                .Select(p=>new PaintProductResponseDto()
                                                                                        {Id=p.Id,
                                                                                        Name=p.Name,
                                                                                        Brand=p.Brand,
                                                                                        Price=p.Price,
                                                                                        Inventory=p.Inventory,
                                                                                        RowVersion=p.RowVersion})
                                                                                .FirstOrDefaultAsync(cancellationToken);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatPaintProduct([FromBody] PaintProductCreateRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            bool existed = await _dbContext.PaintProducts.AnyAsync(p=>p.Name == request.Name.Trim(), cancellationToken);
            if (existed)
            {
                return Conflict("this name paintproduct is existed");
            }

            PaintProduct paintProduct = new PaintProduct(request.Name, request.Price, request.Brand, request.Inventory);

            _dbContext.PaintProducts.Add(paintProduct);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqlException sqlException
                        && sqlException.Number == 2601)
            {
                return Conflict("this name paintproduct is existed");
            }

            return CreatedAtAction(nameof(GetPaintProduct), new {paintProduct.Id}, 
                                    new PaintProductResponseDto(){Id=paintProduct.Id,
                                                                    Name=paintProduct.Name,
                                                                    Brand=paintProduct.Brand,
                                                                    Price=paintProduct.Price,
                                                                    Inventory=paintProduct.Inventory,
                                                                    RowVersion=paintProduct.RowVersion});
        }
    }
}
