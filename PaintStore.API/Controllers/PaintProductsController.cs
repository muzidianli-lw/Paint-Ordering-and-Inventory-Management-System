using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Application.Common;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Services;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaintProductsController : ControllerBase
    {
        private readonly PaintProductsService _dbService;
        public PaintProductsController(PaintProductsService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public async Task<ActionResult> GetKeysetPagination([FromQuery] PaginationKeysetRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            PaginationKeysetResult<PaintProductResult> response = 
                await _dbService.GetKeysetPaginationAsync(request.PageSize, request.LastPageEndId, cancellationToken);

            return Ok(new PaginationKeysetResponseDto<PaintProductResult>()
                        {
                            Items = response.Items,
                            HasNextPage = response.HasNextPage,
                            ThisPageEndId = response.ThisPageEndId,
                            PageSize = request.PageSize
                        });
        }

        [HttpGet("page")]
        public async Task<ActionResult> GetOffsetPagination([FromQuery] PaginationOffsetRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            ServiceResult<PaginationOffsetQueryResult<PaintProductResult>> 
                response = await _dbService.GetOffsetPaginationAsync(request.Page,
                                                                    request.PageSize,
                                                                    cancellationToken);
            
            return response.State switch
                {
                    ServiceResultsEnum.InvalidValue => BadRequest(response.ErrorMsg),
                    ServiceResultsEnum.Success => Ok(new PaginationOffsetResponseDto<PaintProductResult> ()
                                                    {
                                                        Items = response.Data!.Items,
                                                        TotalCount = response.Data.TotalCount,
                                                        Page = request.Page,
                                                        PageSize = request.PageSize
                                                    }),
                    _ => throw new InvalidOperationException()
                };
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<ActionResult> DeletePaintProduct([FromRoute] int id, CancellationToken cancellationToken)
        {
            ServiceResultsEnum response = await _dbService.DeletePaintProductAsync(id, cancellationToken);
            return response switch
                {
                    ServiceResultsEnum.Success => NoContent(),
                    ServiceResultsEnum.NotExisted => NotFound(),
                    ServiceResultsEnum.RelatedDataExisted => Conflict(ErrorCodes.HasRelatedData),
                    _ => throw new InvalidOperationException()
                };
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetPaintProduct([FromRoute] int id, CancellationToken cancellationToken)
        {
            ServiceResult<PaintProductResult> response = await _dbService.GetPaintProductAsync(id, cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.Success => Ok(response.Data),
                ServiceResultsEnum.NotExisted => NotFound(),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update([FromRoute] int id,
                                                [FromBody] PaintProductUpdateRequestDto request,
                                                CancellationToken cancellationToken)
        {
            ServiceResult<PaintProductResult> response = await _dbService.UpdateAsync(id,
                                                                                    request.Name,
                                                                                    request.Price,
                                                                                    request.Brand,
                                                                                    request.Inventory,
                                                                                    request.RowVersion,
                                                                                    cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(),
                ServiceResultsEnum.NameExisted=> Conflict(ErrorCodes.NameAlreadyExists),
                ServiceResultsEnum.OldDataChanged => Conflict(ErrorCodes.VersionConflict),
                ServiceResultsEnum.Success => Ok(response.Data),
                _=> throw new InvalidOperationException()
            };
        }

        [HttpPost]
        public async Task<IActionResult> CreatPaintProduct([FromBody] PaintProductCreateRequestDto request, 
                                                            CancellationToken cancellationToken)
        {
            ServiceResult<PaintProductResult> response = await _dbService.CreatPaintProductAsync(request.Name,
                                                                                                request.Price,
                                                                                                request.Brand,
                                                                                                request.Inventory,
                                                                                                cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.NameExisted => Conflict(ErrorCodes.NameAlreadyExists),
                ServiceResultsEnum.Success => 
                    CreatedAtAction(nameof(GetPaintProduct), new {response.Data!.Id}, response.Data),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
