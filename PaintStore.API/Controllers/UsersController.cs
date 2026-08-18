using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Application.Users;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Application.Common;
using PaintStore.API.Services;

namespace PaintStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationOffsetRequestDto request, 
                                                        CancellationToken cancellationToken)
        {
            ServiceResult<PaginationOffsetQueryResult<UserResult>> serviceResult 
                = await _usersService.GetAllUsers(request.Page, request.PageSize, cancellationToken);
            
            PaginationOffsetResponseDto<UserResult>? response = null;
            if (serviceResult.State == ServiceResultsEnum.Success)
            {
                response = new PaginationOffsetResponseDto<UserResult>()
                                            {
                                                Items = serviceResult.Data.Items,
                                                TotalCount = serviceResult.Data != null
                                                                ? serviceResult.Data.TotalCount
                                                                : 0,
                                                Page = request.Page,
                                                PageSize = request.PageSize
                                            };
            }

            return serviceResult.State switch
            {
                ServiceResultsEnum.Success => Ok(response),
                ServiceResultsEnum.InvalidValue => BadRequest(serviceResult.ErrorMsg),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            ServiceResult<UserResult> response = 
                await _usersService.GetUserQueryByIdAsync(id, cancellationToken);

            return response.State switch
            {
                ServiceResultsEnum.Success => Ok(response.Data),
                ServiceResultsEnum.NotExisted => NotFound(),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Put([FromRoute] int id, 
                                                [FromBody] UserUpdateRequestDto userDto, 
                                                CancellationToken cancellationToken)
        {
            ServiceResult<UserResult> response = await _usersService.UpdateUserInfoAsync(id,
                                                                                            userDto.Name,
                                                                                            userDto.Email,
                                                                                            userDto.Phone,
                                                                                            userDto.RowVersion,
                                                                                            cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.Success => Ok(response.Data),
                ServiceResultsEnum.NotExisted => NotFound(),
                ServiceResultsEnum.EmailExisted => Conflict(),
                ServiceResultsEnum.OldDataChanged => Conflict(),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            ServiceResult<UserResult> response = 
                await _usersService.DeleteUserAsync(id, cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.Success => NoContent(),
                ServiceResultsEnum.RelatedDataExisted => Conflict(),
                ServiceResultsEnum.NotExisted => NotFound(),
                _ => throw new InvalidOperationException()
            };
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateRequestDto userDto, 
                                                CancellationToken cancellationToken)
        {
            ServiceResult<UserResult> response = await _usersService.CreateUserAsync(
                                                                            userDto.Email,
                                                                            userDto.Name,
                                                                            userDto.Phone, 
                                                                            cancellationToken);
            return response.State switch
            {
                ServiceResultsEnum.Success => 
                    CreatedAtAction(nameof(Get), new {id=response.Data.Id}, response.Data),
                ServiceResultsEnum.EmailExisted => Conflict(),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
