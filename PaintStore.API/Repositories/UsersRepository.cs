using System.Data;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Application.Users;
using PaintStore.API.Database;
using PaintStore.API.Application.Common;
using PaintStore.Models;

namespace PaintStore.API.Repositories;

public class UsersRepository
{
    private readonly PaintStoreDbContext _dbContext;

    public UsersRepository(PaintStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationOffsetQueryResult<UserResult>> 
        GetPaginationOffsetInfoAsync(int offset, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.OrderBy(u=>u.Id);
        await using var transaction = 
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot,
                                                            cancellationToken);

        List<UserResult> users = await query.Skip(offset)
                                                .Take(pageSize)
                                                .Select(UserResult.Projection)
                                                .ToListAsync(cancellationToken);
        
        int totalCount = await query.CountAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PaginationOffsetQueryResult<UserResult>(){
                                    Items=users,
                                    TotalCount = totalCount};
    }

    public async Task<UserResult?> GetUserQueryByIdAsync(int id,
                                                        CancellationToken cancellationToken)
    {
        return await _dbContext.Users.Where(u=>u.Id == id)
                                        .Select(UserResult.Projection)
                                        .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.Where(u=>u.Id == id).FirstOrDefaultAsync(cancellationToken);
    }  

    public async Task<bool> CheckEmailExistedForOtherIdsAsync(string email,
                                                            int id, 
                                                            CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u=>u.Email == email.Trim() && u.Id != id,
                                                cancellationToken);
    }

    public async Task<bool> CheckEmailExistedAsync(string email,
                                                    CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u=>u.Email == email.Trim(), cancellationToken);
    }

    public async Task<bool> CheckUserExistedByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public void SetRowVersion(User user, byte[] rowVersion)
    {
        _dbContext.Entry(user).Property(u=>u.RowVersion).OriginalValue = rowVersion;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.Where(u=>u.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public void AddUser(User user)
    {
        _dbContext.Users.Add(user);
    }
}
