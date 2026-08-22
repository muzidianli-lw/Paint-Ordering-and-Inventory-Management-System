using PaintStore.API.Application.Users;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;
using PaintStore.API.Application.Common;
using PaintStore.Models;

namespace PaintStore.API.Services;

public class UsersService
{
    private readonly UsersRepository _usersRepository;
    private readonly UnitOfWork _unitOfWork;

    public UsersService(UsersRepository usersRepository, UnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _unitOfWork = unitOfWork;
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
                        ErrorMsg = ErrorCodes.TooLargePage
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
                                ErrorMsg = ErrorCodes.UserNotExists
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
                                    ErrorMsg = ErrorCodes.UserNotExists
                                };
        }

        bool emailExist = await _usersRepository.CheckEmailExistedForOtherIdsAsync(email, id, cancellationToken); 
        if (emailExist)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.EmailExisted,
                        ErrorMsg = ErrorCodes.EmailAlreadyExists
                    };
        }

        _usersRepository.SetExpectedRowVersion(user, rowVersion);
        user.Update(name, email, phone);

        RepositoryResultsEnum response = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (response == RepositoryResultsEnum.ConcurrencyException)
        {
            bool userExist = await _usersRepository.CheckUserExistedByIdAsync(id, cancellationToken);
            if (userExist)
            {
                return new ServiceResult<UserResult>()
                        {
                            State = ServiceResultsEnum.OldDataChanged,
                            ErrorMsg = ErrorCodes.VersionConflict
                        };
            }
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.NotExisted,
                        ErrorMsg = ErrorCodes.UserNotExists
                    };
        }
        else if (response == RepositoryResultsEnum.UniqueIndexDuplicated)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.EmailExisted,
                        ErrorMsg = ErrorCodes.EmailAlreadyExists
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
        RepositoryResults<int> response = 
                    await _usersRepository.DeleteUserAsync(id, cancellationToken);
        if (response.ResultsEnum == RepositoryResultsEnum.Success 
                && response.Data == 0)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.NotExisted,
                        ErrorMsg = ErrorCodes.UserNotExists
                    };
        }
        else if (response.ResultsEnum == RepositoryResultsEnum.ForeignKeyConstraintViolation)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.RelatedDataExisted,
                        ErrorMsg = ErrorCodes.HasRelatedData
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
                    ErrorMsg = ErrorCodes.EmailAlreadyExists
                };
        }

        User user = new User(name, email, phone);
        _usersRepository.AddUser(user);


        RepositoryResultsEnum response = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if(response == RepositoryResultsEnum.UniqueIndexDuplicated)
        {
            return new ServiceResult<UserResult>()
                    {
                        State = ServiceResultsEnum.EmailExisted,
                        ErrorMsg = ErrorCodes.EmailAlreadyExists
                    };
        }

        return new ServiceResult<UserResult>(){
                                    State = ServiceResultsEnum.Success,
                                    Data = UserResult.FromEntity(user)};
    }
}
