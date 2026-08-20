using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Application.Common;
using PaintStore.API.Application.Orders;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Services;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersService _ordersService;

        public OrdersController(OrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpGet("page")]
        public async Task<ActionResult<PaginationOffsetResponseDto<OrderResult>>>
            GetSpecificPage([FromQuery] PaginationOffsetRequestDto request,
                            CancellationToken cancellationToken)
        {
            ServiceResult<PaginationOffsetQueryResult<OrderResult>> response = 
                await _ordersService.GetSpecificPageAsync(request.Page, request.PageSize, cancellationToken);

            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.InvalidValue => BadRequest(response.ErrorMsg),
                ServiceResultsEnum.Success => Ok(new PaginationOffsetResponseDto<OrderResult>()
                                                    {
                                                        Items=response.Data!.Items,
                                                        TotalCount=response.Data.TotalCount,
                                                        Page=request.Page,
                                                        PageSize=request.PageSize
                                                    }),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpGet]
        public async Task<ActionResult<PaginationKeysetResponseDto<OrderResult>>> 
            GetNextPage([FromQuery] PaginationKeysetRequestDto request,
                        CancellationToken cancellationToken)
        {
            PaginationKeysetResult<OrderResult> Response = await _ordersService.GetNextPageAsync(
                request.LastPageEndId, request.PageSize, cancellationToken);

            return Ok(new PaginationKeysetResponseDto<OrderResult> ()
                {
                    Items=Response.Items,
                    HasNextPage=Response.HasNextPage,
                    ThisPageEndId=Response.ThisPageEndId,
                    PageSize=request.PageSize
                });
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<ActionResult> DeleteOrder([FromRoute] int id,
                                                    CancellationToken cancellationToken)
        {
            ServiceResultsEnum response =  await _ordersService.DeleteOrderAsync(id, cancellationToken);
            return response switch
            {
                ServiceResultsEnum.Success => NoContent(),
                ServiceResultsEnum.NotExisted => NotFound(),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<ActionResult<OrderResult>> UpdateOrder([FromRoute] int id,
                                                    [FromBody] OrderUpdateRequestDto request,
                                                    CancellationToken cancellationToken)
        {
            ServiceResult<OrderResult> response = await _ordersService.UpdateOrderAsync(id,
                                                                                        request.RowVersion,
                                                                                        request.OrderItems,
                                                                                        cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.NotEnough => Conflict(response.ErrorMsg),
                ServiceResultsEnum.OldDataChanged => Conflict(response.ErrorMsg),
                ServiceResultsEnum.RelatedDataNotExisted => Conflict(response.ErrorMsg),
                ServiceResultsEnum.Success => Ok(response.Data),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<OrderResult>> GetOrderByOrderId([FromRoute] int id,
                                                        CancellationToken cancellationToken)
        {
            ServiceResult<OrderResult> response = 
                await _ordersService.GetOrderByOrderIdAsync(id, cancellationToken);

            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.Success => Ok(response.Data),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpGet("byUser/{id:int:min(1)}")]
        public async Task<ActionResult<PaginationOffsetResponseDto<OrderResult>>> 
            GetOrdersByUserId(
                [FromRoute] int id,
                [FromQuery] PaginationOffsetRequestDto request,
                CancellationToken cancellationToken)
        {
            ServiceResult<PaginationOffsetQueryResult<OrderResult>> response = 
                await _ordersService.GetOrdersByUserIdAsync(id, request.Page, request.PageSize, cancellationToken);

            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.InvalidValue => BadRequest(response.ErrorMsg),
                ServiceResultsEnum.Success => Ok(new PaginationOffsetResponseDto<OrderResult>()
                                                    {
                                                        Items=response.Data!.Items,
                                                        TotalCount=response.Data.TotalCount,
                                                        Page=request.Page,
                                                        PageSize=request.PageSize
                                                    }),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto request,
                                                    CancellationToken cancellationToken)
        {
            ServiceResult<OrderResult> response =  await _ordersService.CreateOrderAsync(request.UserId,
                                                                                        request.Items,
                                                                                        cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.NotExisted => NotFound(response.ErrorMsg),
                ServiceResultsEnum.NotEnough => Conflict(response.ErrorMsg),
                ServiceResultsEnum.Success => 
                CreatedAtAction(nameof(GetOrderByOrderId), new {response.Data!.Id}, response.Data),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
