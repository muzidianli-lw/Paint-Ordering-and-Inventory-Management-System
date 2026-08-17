using PaintStore.API.Application.Users;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;
using PaintStore.API.Application.Common;
using PaintStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace PaintStore.API.Services;

public class UsersService
{
    private readonly UsersRepository _usersRepository;

    public UsersService(UsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<ServiceResult<PaginationOffsetQueryResult<UserResult>>> 
        GetAllUsers(int page, int pageSize, CancellationToken cancellationToken)
    {
        long offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new ServiceResult<PaginationOffsetQueryResult<UserResult>>()
                    {
                        State = ServiceResultsEnum.InvalidValue,
                        ErrorMsg = PageErrorCodes.TooLargePage
                    };
        }

        PaginationOffsetQueryResult<UserResult> result =  
            await _usersRepository.GetPaginationOffsetInfoAsync((int)offset, pageSize, cancellationToken);
        
        return new ServiceResult<PaginationOffsetQueryResult<UserResult>>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = result
                    };
    }

    public async Task<ServiceResult<UserResult>> GetUserQueryByIdAsync(int id, 
                                                                    CancellationToken cancellationToken)
    {
            UserResult? user = await _usersRepository.GetUserQueryByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return new ServiceResult<UserResult>()
                            {
                                State = ServiceResultsEnum.NotExisted,
                                ErrorMsg = UserErrorCodes.UserNotExists
                            };
            }
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = user
                    };
    }

    public async Task<ServiceResult<UserResult>> UpdateUserInfoAsync(int id,
                                                                        string name,
                                                                        string email,
                                                                        string phone,
                                                                        byte[] rowVersion,
                                                                        CancellationToken cancellationToken)
    {
        User? user = await _usersRepository.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return new ServiceResult<UserResult>()
                                {
                                    State = ServiceResultsEnum.NotExisted,
                                    ErrorMsg = UserErrorCodes.UserNotExists
                                };
        }

        bool emailExist = await _usersRepository.CheckEmailExistedForOtherIdsAsync(email, id, cancellationToken); 
        if (emailExist)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.EmailExisted,
                        ErrorMsg = UserErrorCodes.EmailAlreadyExists
                    };
        }

        _usersRepository.SetRowVersion(user, rowVersion);
        user.Update(name, email, phone);

        try
        {
            await _usersRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            bool userExist = await _usersRepository.CheckUserExistedByIdAsync(id, cancellationToken);
            if (userExist)
            {
                return new ServiceResult<UserResult>()
                        {
                            State = ServiceResultsEnum.OldDataChanged,
                            ErrorMsg = UserErrorCodes.VersionConflict
                        };
            }
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.NotExisted,
                        ErrorMsg = UserErrorCodes.UserNotExists
                    };
        }
        catch (DbUpdateException exception)
            when(exception.InnerException is SqlException sqlException 
                && sqlException.Number == 2601)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.EmailExisted,
                        ErrorMsg = UserErrorCodes.EmailAlreadyExists
                    };
        }
        return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = UserResult.FromEntity(user)
                    };
    }

    public async Task<ServiceResult<UserResult>> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            int affectedRows = await _usersRepository.DeleteUserAsync(id, cancellationToken);
            if(affectedRows == 0)
            {
                return new ServiceResult<UserResult>()
                        {
                            State = ServiceResultsEnum.NotExisted,
                            ErrorMsg = UserErrorCodes.UserNotExists
                        };
            }
        }
        catch(SqlException sqlException)
            when(sqlException.Number == 547)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.RelatedDataExisted,
                        ErrorMsg = UserErrorCodes.HasRelatedData
                    };
        }
        return new ServiceResult<UserResult>(){ State = ServiceResultsEnum.Success };
    }

    public async Task<ServiceResult<UserResult>> CreateUserAsync(string email,
                                                                    string name,
                                                                    string phone, 
                                                                    CancellationToken cancellationToken)
    {
        bool emailExist = await _usersRepository.CheckEmailExistedAsync(email, cancellationToken);
        if (emailExist)
        {
            return new ServiceResult<UserResult>()
                {
                    State = ServiceResultsEnum.EmailExisted,
                    ErrorMsg = UserErrorCodes.EmailAlreadyExists
                };
        }

        User user = new User(name, email, phone);
        _usersRepository.AddUser(user);

        try
        {
            await _usersRepository.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateException exception)
            when(exception.InnerException is SqlException sqlException
                && sqlException.Number == 2601)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.EmailExisted,
                        ErrorMsg = UserErrorCodes.EmailAlreadyExists
                    };
        }

        return new ServiceResult<UserResult>(){
                                    State = ServiceResultsEnum.Success,
                                    Data = UserResult.FromEntity(user)};
    }
}
