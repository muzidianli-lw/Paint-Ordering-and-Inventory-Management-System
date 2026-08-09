using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        public async Task<ActionResult> GetAllPaintProducts([FromQuery] PaginationRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            long startIndex = ((long)request.Page - 1) * request.PageSize;
            if (startIndex > int.MaxValue)
            {
                return BadRequest("page input error");
            }

            List<PaintProductResponseDto> items = await _dbContext.PaintProducts
                                                        .OrderBy(p=>p.Id)
                                                        .Skip((int)startIndex)
                                                        .Take(request.PageSize)
                                                        .Select(p=>new PaintProductResponseDto()
                                                                                        {Id=p.Id,
                                                                                        Name=p.Name,
                                                                                        Brand=p.Brand,
                                                                                        Price=p.Price,
                                                                                        Inventory=p.Inventory,
                                                                                        RowVersion=p.RowVersion})
                                                        .ToListAsync(cancellationToken);

            int totalCount = await _dbContext.PaintProducts.CountAsync(cancellationToken);

            return Ok(new PaginationResponseDto<PaintProductResponseDto> (){Items = items,
                                                                            TotalCount=totalCount,
                                                                            Page = request.Page,
                                                                            PageSize = request.PageSize});
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

        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> update([FromRoute] int id,
                                                [FromBody] PaintProductUpdateRequestDto request,
                                                CancellationToken cancellationToken)
        {
            PaintProduct? paintProduct = await _dbContext.PaintProducts.FirstOrDefaultAsync(p=>p.Id==id, cancellationToken);
            if (paintProduct == null)
            {
                return NotFound();
            }
            bool existed = await _dbContext.PaintProducts.AnyAsync(p=>p.Name == request.Name.Trim() && p.Id != id,
                                                                    cancellationToken);
            if (existed)
            {
                return Conflict("this name paintproduct is existed");
            }

            paintProduct.Update(request.Name, request.Price, request.Brand, request.Inventory);
            _dbContext.Entry(paintProduct)
                    .Property(p=>p.RowVersion)
                    .OriginalValue = request.RowVersion;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                existed = await _dbContext.PaintProducts.AnyAsync(p=>p.Id==id, cancellationToken);
                if (!existed)
                {
                    return NotFound();
                }
                return Conflict("old data changed");
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqlException sqlException
                        && sqlException.Number == 2601)
            {
                return Conflict("this name paintproduct is existed");
            }

            return Ok(new PaintProductResponseDto(){Id=paintProduct.Id,
                                                    Name=paintProduct.Name,
                                                    Brand=paintProduct.Brand,
                                                    Price=paintProduct.Price,
                                                    Inventory=paintProduct.Inventory,
                                                    RowVersion=paintProduct.RowVersion});
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
