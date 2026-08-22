using PaintStore.API.Application.Common;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;
using PaintStore.Models;

namespace PaintStore.API.Services;

public class PaintProductsService
{
    private readonly PaintProductsRepository _paintProductsRepository;
    private readonly UnitOfWork _unitOfWork;

    public PaintProductsService(PaintProductsRepository paintProductsRepository, UnitOfWork unitOfWork)
    {
        _paintProductsRepository = paintProductsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginationKeysetResult<PaintProductResult>>
        GetKeysetPaginationAsync(int pageSize, int? lastPageEndId, CancellationToken cancellationToken)
    {
        List<PaintProductResult> response = 
            await _paintProductsRepository.GetKeysetPaginationAsync(pageSize, lastPageEndId, cancellationToken);
            
        bool hasNextPage = response.Count() > pageSize;
        if(hasNextPage)
        {
            response.RemoveAt(pageSize);   
        }
        int? thisPageEndId = hasNextPage ? response[^1].Id: null;           

        return new PaginationKeysetResult<PaintProductResult>()
                {
                    Items = response,
                    HasNextPage = hasNextPage,
                    ThisPageEndId = thisPageEndId
                };
    }

    public async Task<ServiceResult<PaginationOffsetQueryResult<PaintProductResult>>>
        GetOffsetPaginationAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        long startIndex = ((long)page - 1) * pageSize;
        if (startIndex > int.MaxValue)
        {
            return new ServiceResult<PaginationOffsetQueryResult<PaintProductResult>>()
                    {
                        State = ServiceResultsEnum.InvalidValue,
                        ErrorMsg = ErrorCodes.TooLargePage
                    };
        }

        PaginationOffsetQueryResult<PaintProductResult> response = 
            await _paintProductsRepository.GetOffsetPaginationAsync(pageSize, (int)startIndex, cancellationToken);

        return new ServiceResult<PaginationOffsetQueryResult<PaintProductResult>>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = response
                    };
    }

    public async Task<ServiceResultsEnum> DeletePaintProductAsync(int id, CancellationToken cancellationToken)
    {
        RepositoryResults<int> response = await _paintProductsRepository.DeleteByIdAsync(id, cancellationToken);
        if (response.ResultsEnum == RepositoryResultsEnum.Success && response.Data == 0)
        {
            return ServiceResultsEnum.NotExisted;
        }
        
        if(response.ResultsEnum == RepositoryResultsEnum.ForeignKeyConstraintViolation)
        {
            return ServiceResultsEnum.RelatedDataExisted;
        }

        return ServiceResultsEnum.Success;
    }

    public async Task<ServiceResult<PaintProductResult>> GetPaintProductAsync(int id, CancellationToken cancellationToken)
    {
        PaintProductResult? response = await _paintProductsRepository.GetPaintProductResultByIdAsync(id, cancellationToken);
        if (response == null)
        {
            return new ServiceResult<PaintProductResult> ()
                    {
                        State = ServiceResultsEnum.NotExisted
                    };
        }

        return new ServiceResult<PaintProductResult> ()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = response
                    };
    }

    public async Task<ServiceResult<PaintProductResult>> UpdateAsync(int id,
                                                                    string name,
                                                                    decimal price,
                                                                    string brand,
                                                                    int inventory,
                                                                    byte[] rowVersion,
                                                                    CancellationToken cancellationToken)
    {
        PaintProduct? paintProduct = await _paintProductsRepository.GetPaintProductByIdAsync(id, cancellationToken);
        if (paintProduct == null)
        {
            return new ServiceResult<PaintProductResult>()
            {
                 State = ServiceResultsEnum.NotExisted
            };
        }

        bool existed = await _paintProductsRepository.CheckeExistedByNameExceptIdAsync(name, id, cancellationToken);
        if (existed)
        {
            return new ServiceResult<PaintProductResult>()
            {
                 State = ServiceResultsEnum.NameExisted
            };
        }

        paintProduct.Update(name, price, brand, inventory);
        _paintProductsRepository.SetExpectedRowVersion(paintProduct, rowVersion);

        RepositoryResultsEnum response = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (response == RepositoryResultsEnum.ConcurrencyException)
        {
            existed = await _paintProductsRepository.CheckeExistedByIdAsync(id, cancellationToken);
            if(!existed)
            {
                return new ServiceResult<PaintProductResult>()
                    {
                        State = ServiceResultsEnum.NotExisted
                    };
            }
            return new ServiceResult<PaintProductResult>()
                {
                    State = ServiceResultsEnum.OldDataChanged
                };
        }

        if (response == RepositoryResultsEnum.UniqueIndexDuplicated)
        {
            return new ServiceResult<PaintProductResult>()
                    {
                        State = ServiceResultsEnum.NameExisted
                    };
        }
        return new ServiceResult<PaintProductResult>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = PaintProductResult.FromEntity(paintProduct)
                    };
    }

    public async Task<ServiceResult<PaintProductResult>> CreatPaintProductAsync(string name,
                                                                                decimal price,
                                                                                string brand,
                                                                                int inventory,
                                                                                CancellationToken cancellationToken)
    {
        bool existed = await _paintProductsRepository.CheckeExistedByNameAsync(name, cancellationToken);
        if (existed)
        {
            return new ServiceResult<PaintProductResult>()
                    {
                        State = ServiceResultsEnum.NameExisted
                    };
        }

        PaintProduct paintProduct = new PaintProduct(name, price, brand, inventory);

        _paintProductsRepository.AddItem(paintProduct);

        RepositoryResultsEnum response = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if(response == RepositoryResultsEnum.UniqueIndexDuplicated)
        {
            return new ServiceResult<PaintProductResult>()
                    {
                        State = ServiceResultsEnum.NameExisted
                    };
        }
        return new ServiceResult<PaintProductResult>()
                {
                    State = ServiceResultsEnum.Success,
                    Data = PaintProductResult.FromEntity(paintProduct)
                };
    }
}
