using System;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using PaintStore.API.Application.Common;
using PaintStore.API.Application.Orders;
using PaintStore.API.DTOs;
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
                                                                    List<OrderItemCreateRequest> orderItemsRequest,
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
        return new ServiceResult<OrderResult>()
                    {
                        State = ServiceResultsEnum.Success,
                        Data = OrderResult.FromEntity(order)
                    };        
    }

    public async Task<ServiceResult<OrderResult>> GetOrderByOrderIdAsync(int orderId,
                                                                        CancellationToken cancellationToken)
    {
        OrderResult? response = await _orderRespository.GetOrderByOrderIdAsync(orderId, cancellationToken);
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
}
