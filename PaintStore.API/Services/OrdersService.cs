using PaintStore.API.Application.Common;
using PaintStore.API.Application.Orders;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;
using PaintStore.Models;

namespace PaintStore.API.Services;

public class OrdersService
{
    private readonly OrdersRepository _orderRespository;
    private readonly UsersRepository _usersRepository;
    private readonly PaintProductsRepository _paintProductsRepository;

    public OrdersService(OrdersRepository ordersRepository,
                            UsersRepository usersRepository,
                            PaintProductsRepository paintProductsRepository)
    {
        _orderRespository = ordersRepository;
        _usersRepository = usersRepository;
        _paintProductsRepository = paintProductsRepository;
    }

    public async Task<ServiceResultsEnum> DeleteOrderAsync(int id,
                                                CancellationToken cancellationToken)
    {
        int affectedRow = await _orderRespository.DeleteOrderByIdAsync(id, cancellationToken);
        if(affectedRow == 0)
        {
            return ServiceResultsEnum.NotExisted;
        }
        return ServiceResultsEnum.Success;
    }

    public async Task<ServiceResult<OrderResult>> CreateOrderAsync(int userId,
                                                                    List<OrderItemRequest> orderItemsRequest,
                                                                    CancellationToken cancellationToken)
    {
        User? user = await _usersRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return new ServiceResult<OrderResult>()
            {
                State = ServiceResultsEnum.NotExisted,
                ErrorMsg = ErrorCodes.UserNotExists
            };
        }

        Dictionary<int, int> newItemIdCnt = orderItemsRequest.GroupBy(i=>i.PaintProductId)
                                                            .ToDictionary(g=>g.Key, g=>g.Sum(i=>i.Quantity));

        List<PaintProduct> paintProducts= await _paintProductsRepository.GetPaintProductByIdRangeAsync(
                                            newItemIdCnt.Keys.ToList(), cancellationToken);

        if (paintProducts.Count != newItemIdCnt.Keys.Count)
        {
            return new ServiceResult<OrderResult>()
            {
                State = ServiceResultsEnum.NotExisted,
                ErrorMsg = ErrorCodes.PaintProductNotExist
            };
        }

        bool notEnough = paintProducts.Any(p=>p.Inventory < newItemIdCnt[p.Id]);
        if (notEnough)
        {
            return new ServiceResult<OrderResult>()
            {
                State = ServiceResultsEnum.NotEnough,
                ErrorMsg = ErrorCodes.PaintProductInentoryNotEnough
            };
        }

        List<OrderItem> orderItems = [];
        foreach(var paintProduct in paintProducts)
        {
            paintProduct.UpdateInventory(-newItemIdCnt[paintProduct.Id]);
            orderItems.Add(new OrderItem(){PaintProductId = paintProduct.Id,
                                            Quantity = newItemIdCnt[paintProduct.Id],
                                            PaintProduct = paintProduct,
                                            UnitPrice = paintProduct.Price});
        }

        Order order = new Order(userId, user, orderItems);
        _orderRespository.AddOrder(order);
        RepositoryResultsEnum response = await _orderRespository.SaveChangesAsync(cancellationToken);
        if(response == RepositoryResultsEnum.ForeignKeyConstraintViolation)
        {
            bool existed = await _usersRepository.CheckUserExistedByIdAsync(userId, cancellationToken);
            if(existed)
            {
                return new ServiceResult<OrderResult>()
                        {
                            State = ServiceResultsEnum.NotExisted,
                            ErrorMsg = ErrorCodes.PaintProductNotExist
                        };
            }
            return new ServiceResult<OrderResult>()
                    {
                        State = ServiceResultsEnum.NotExisted,
                        ErrorMsg = ErrorCodes.UserNotExists
                    };
        }
        if (response == RepositoryResultsEnum.ConcurrencyException)
        {
            return new ServiceResult<OrderResult>
            {
                State = ServiceResultsEnum.OldDataChanged,
                ErrorMsg = ErrorCodes.VersionConflict
            };
        }
        return new ServiceResult<OrderResult>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = OrderResult.FromEntity(order)
                    };        
    }

    public async Task<ServiceResult<OrderResult>> GetOrderByOrderIdAsync(int orderId,
                                                                        CancellationToken cancellationToken)
    {
        OrderResult? response = await _orderRespository.GetOrderResultByOrderIdAsync(orderId, cancellationToken);
        if (response == null)
        {
            return new ServiceResult<OrderResult>()
            {
                State = ServiceResultsEnum.NotExisted,
                ErrorMsg = ErrorCodes.OrderNotExist
            };
        }
        return new ServiceResult<OrderResult>()
        {
            State = ServiceResultsEnum.Success,
            Data = response
        };
    }

    public async Task<ServiceResult<OrderResult>> UpdateOrderAsync(int orderId,
                                                                    byte[] rowVersion,
                                                                    List<OrderItemRequest> orderItems,
                                                                    CancellationToken cancellationToken)
    {
        Order? order = await _orderRespository.GetDetailedOrderByOrderIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return new ServiceResult<OrderResult>()
                    {
                        State = ServiceResultsEnum.NotExisted,
                        ErrorMsg = ErrorCodes.OrderNotExist
                    };
        }

        Dictionary<int, int> oldIdToCnt = order.OrderItems.ToDictionary(o=>o.PaintProductId, o=>o.Quantity);
        Dictionary<int, int> newIdToCnt = orderItems.GroupBy(i=>i.PaintProductId)
                                                    .ToDictionary(g=>g.Key, g=>-g.Sum(i=>i.Quantity));
        Dictionary<int, int> delta = oldIdToCnt.Concat(newIdToCnt)
                                            .GroupBy(i=>i.Key)
                                            .ToDictionary(g=>g.Key, g=>g.Sum(i=>i.Value));


        List<PaintProduct> PaintProducts = await _paintProductsRepository.GetPaintProductByIdRangeAsync(
                                            newIdToCnt.Keys.Concat(oldIdToCnt.Keys).ToList(), cancellationToken);

        if (PaintProducts.Count != delta.Keys.Count)
        {
            return new ServiceResult<OrderResult>()
                    {
                        State = ServiceResultsEnum.RelatedDataNotExisted,
                        ErrorMsg = ErrorCodes.PaintProductNotExist
                    };
        }

        bool notEnough = PaintProducts.Any(p=>p.Inventory + delta[p.Id] < 0);
        if (notEnough)
        {
            return new ServiceResult<OrderResult>()
            {
                State = ServiceResultsEnum.NotEnough,
                ErrorMsg = ErrorCodes.PaintProductInentoryNotEnough
            };
        }

        foreach (var p in PaintProducts)
        {
            p.UpdateInventory(delta[p.Id]);
        }

        List<OrderItem> items = PaintProducts
                        .Where(p=>newIdToCnt.Keys.Contains(p.Id))
                        .Select(p=>new OrderItem(){PaintProductId = p.Id,
                                                Quantity = -newIdToCnt[p.Id],
                                                PaintProduct = p,
                                                UnitPrice = p.Price})
                        .ToList();       

        order.Update(items);

        _orderRespository.SetExpectedOriginalValue(order, rowVersion);
        
        RepositoryResultsEnum response = await _orderRespository.SaveChangesAsync(cancellationToken);
        if (response == RepositoryResultsEnum.ConcurrencyException)
        {
            bool existed = await _orderRespository.CheckOrderExistedByOrderIdAsync(orderId, cancellationToken);
            if(existed)
            {
                return new ServiceResult<OrderResult>()
                        {
                            State = ServiceResultsEnum.OldDataChanged,
                            ErrorMsg = ErrorCodes.VersionConflict
                        };  
            }
            return new ServiceResult<OrderResult>()
                        {
                            State = ServiceResultsEnum.NotExisted,
                            ErrorMsg = ErrorCodes.OrderNotExist
                        };  
        }

        if(RepositoryResultsEnum.ForeignKeyConstraintViolation == response)
        {
            return new ServiceResult<OrderResult>()
                {
                    State = ServiceResultsEnum.RelatedDataNotExisted,
                    ErrorMsg = ErrorCodes.PaintProductNotExist
                };
        }

        return new ServiceResult<OrderResult>()
                {
                    State = ServiceResultsEnum.Success,
                    Data = OrderResult.FromEntity(order)
                };
    }

    public async Task<ServiceResult<PaginationOffsetQueryResult<OrderResult>>>
        GetOrdersByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        bool userExisted = await _usersRepository.CheckUserExistedByIdAsync(userId, cancellationToken);
        if(!userExisted)
        {
            return new ServiceResult<PaginationOffsetQueryResult<OrderResult>>()
                    {
                        State = ServiceResultsEnum.NotExisted,
                        ErrorMsg = ErrorCodes.UserNotExists
                    };
        }

        return await GetSpecificPageImpAsync(page, pageSize, userId, cancellationToken);
    }

    public async Task<ServiceResult<PaginationOffsetQueryResult<OrderResult>>> GetSpecificPageAsync(
                                    int page, int pageSize, CancellationToken cancellationToken)
    {
        return await GetSpecificPageImpAsync(page, pageSize, null, cancellationToken);
    }

    private async Task<ServiceResult<PaginationOffsetQueryResult<OrderResult>>> GetSpecificPageImpAsync(
                            int page, int pageSize, int? userId, CancellationToken cancellationToken)
    {
        long offset = ((long)page - 1) * pageSize;
        if(offset > int.MaxValue)
        {
            return new ServiceResult<PaginationOffsetQueryResult<OrderResult>>()
                    {
                        State = ServiceResultsEnum.InvalidValue,
                        ErrorMsg = ErrorCodes.TooLargePage
                    };
        }

        PaginationOffsetQueryResult<OrderResult> response = await _orderRespository.GetPaginationOffsetInfoAsync(
                                                                    (int)offset, pageSize, userId, cancellationToken);

        return new ServiceResult<PaginationOffsetQueryResult<OrderResult>> ()
                {
                    State = ServiceResultsEnum.Success,
                    Data = new PaginationOffsetQueryResult<OrderResult>()
                            {
                                Items=response.Items,
                                TotalCount=response.TotalCount
                            }
                };
    }

    public async Task<PaginationKeysetResult<OrderResult>>
            GetNextPageAsync(int? lastPageEndId, int pageSize, CancellationToken cancellationToken)
    {
        List<OrderResult> orders = await _orderRespository.GetNextPageAsync(lastPageEndId, pageSize, cancellationToken);

        bool hasNextPage = orders.Count > pageSize;
        if(hasNextPage)
        {
            orders.RemoveAt(pageSize);
        }
        int? thisPageEndId = orders.Count > 0 ? orders[^1].Id : null;

        return new PaginationKeysetResult<OrderResult> ()
                {
                    Items=orders,
                    HasNextPage=hasNextPage,
                    ThisPageEndId=thisPageEndId
                };
    }
}
