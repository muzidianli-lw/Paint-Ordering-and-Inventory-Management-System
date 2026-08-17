using System;
using Microsoft.AspNetCore.Mvc;
using PaintStore.API.Application.Users;
using PaintStore.API.DTOs;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;
using PaintStore.API.Application.Common;

namespace PaintStore.API.Services;

public class UsersService
{
    private readonly UsersRepository _usersRepository;

    public UsersService(UsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<ServiceResult<PaginationOffsetQueryResult<UserQueryResult>>> 
        GetAllUsers(int page, int pageSize, CancellationToken cancellationToken)
    {
        long offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new ServiceResult<PaginationOffsetQueryResult<UserQueryResult>>()
                    {
                        State = ServiceResultsEnum.InvalidValue,
                        ErrorMsg = "Page is too large."
                    };
        }

        PaginationOffsetQueryResult<UserQueryResult> result =  
            await _usersRepository.GetPaginationOffsetInfoAsync((int)offset, pageSize, cancellationToken);
        
        return new ServiceResult<PaginationOffsetQueryResult<UserQueryResult>>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = result
                    };
    }
}
