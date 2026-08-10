using System.Data;
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
        public async Task<ActionResult> GetKeysetPagination([FromQuery] PaginationKeysetRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            IQueryable<PaintProduct> query = _dbContext.PaintProducts;
            if(request.LastPageEndId is int lastPageEndId)
            {
                query = query.Where(p=>p.Id > lastPageEndId);
            }

            List<PaintProductResponseDto> paintProducts = await query.OrderBy(p=>p.Id)
                                                                    .Take(request.PageSize + 1)
                                                                    .Select(p=>new PaintProductResponseDto()
                                                                                {Id=p.Id,
                                                                                Name=p.Name,
                                                                                Brand=p.Brand,
                                                                                Price=p.Price,
                                                                                Inventory=p.Inventory,
                                                                                RowVersion=p.RowVersion
                                                                                })
                                                                    .ToListAsync(cancellationToken);
            
            bool hasNextPage = paintProducts.Count() > request.PageSize;
            if(hasNextPage)
            {
                paintProducts.RemoveAt(request.PageSize);   
            }
            int? thisPageEndId = hasNextPage ? paintProducts[^1].Id: null;           

            return Ok(new PaginationKeysetResponseDto<PaintProductResponseDto>()
                        {   
                            Items = paintProducts,
                            HasNextPage=hasNextPage,
                            ThisPageEndId=thisPageEndId
                        });
        }

        [HttpGet("page")]
        public async Task<ActionResult> GetOffsetPagination([FromQuery] PaginationOffsetRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            long startIndex = ((long)request.Page - 1) * request.PageSize;
            if (startIndex > int.MaxValue)
            {
                return BadRequest("page input error");
            }

            await using var transaction = 
                await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot, 
                                                                cancellationToken);

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

            await transaction.CommitAsync(cancellationToken);

            return Ok(new PaginationOffsetResponseDto<PaintProductResponseDto> (){Items = items,
                                                                            TotalCount=totalCount,
                                                                            Page = request.Page,
                                                                            PageSize = request.PageSize});
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> DeletePaintProduct([FromRoute] int id, CancellationToken cancellationToken)
        {
            try
            {
                int affectedRow = await _dbContext.PaintProducts.Where(p=>p.Id == id).ExecuteDeleteAsync(cancellationToken);
                if (affectedRow == 0)
                {
                    return NotFound();
                }
            }
            catch (SqlException sqlException)
                when(sqlException.Number == 547)
            {
                    return Conflict("related data exsisted");
            }
            return NoContent();
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
