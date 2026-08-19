using System;
using PaintStore.API.Application.Common;
using PaintStore.API.Enums;
using PaintStore.API.Repositories;

namespace PaintStore.API.Services;

public class OrdersService
{
    private readonly OrdersRepository _orderRespository;

    public OrdersService(OrdersRepository ordersRepository)
    {
        _orderRespository = ordersRepository;
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
}
