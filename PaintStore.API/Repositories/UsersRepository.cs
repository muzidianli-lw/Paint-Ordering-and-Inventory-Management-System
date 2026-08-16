using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.DTOs;
using PaintStore.API.Repositories.Results;

namespace PaintStore.API.Repositories;

public class UsersRepository
{
    private readonly PaintStoreDbContext _dbContext;

    public UsersRepository(PaintStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationOffsetResult<UserResponseDto>> 
        GetPaginationOffsetInfoAsync(int offset, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.OrderBy(u=>u.Id);
        await using var transaction = 
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        List<UserResponseDto> users = await query.Skip(offset)
                                                .Take(pageSize)
                                                .Select(UserResponseDto.Projection)
                                                .ToListAsync(cancellationToken);
        
        int totalCount = await query.CountAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PaginationOffsetResult<UserResponseDto>(){Items=users, TotalCount = totalCount};
    }

}
