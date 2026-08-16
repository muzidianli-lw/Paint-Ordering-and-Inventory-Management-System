using System;
using Microsoft.AspNetCore.Mvc;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;
using PaintStore.API.Repositories.Results;
using PaintStore.API.Services.Results;

namespace PaintStore.API.Services;

public class UsersService
{
    private readonly UsersRepository _usersRepository;

    public UsersService(UsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<ServiceResult<PaginationOffsetResponseDto<UserResponseDto>>> 
        GetAllUsers(PaginationOffsetRequestDto request, CancellationToken cancellationToken)
    {
        long offset = ((long)request.Page - 1) * request.PageSize;
        if (offset > int.MaxValue)
        {
            return new ServiceResult<PaginationOffsetResponseDto<UserResponseDto>>()
                    {
                        State = ServiceResultsEnum.BadRequest,
                        ErrorMsg = "Page is too large."
                    };
        }

        PaginationOffsetResult<UserResponseDto> paginationOffsetResult =  
            await _usersRepository.GetPaginationOffsetInfoAsync((int)offset, request.PageSize, cancellationToken);
        
        var response = new PaginationOffsetResponseDto<UserResponseDto>()
                        {
                            Items = paginationOffsetResult.Items,
                            Page = request.Page,
                            PageSize = request.PageSize,
                            TotalCount = paginationOffsetResult.TotalCount
                        };

        return new ServiceResult<PaginationOffsetResponseDto<UserResponseDto>>()
                    {
                        State = ServiceResultsEnum.Success,
                        Obj = response
                    };
    }
}
