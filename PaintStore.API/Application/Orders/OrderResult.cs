using System;
using System.Linq.Expressions;
using PaintStore.API.DTOs;
using PaintStore.Models;

namespace PaintStore.API.Application.Orders;

public class OrderResult
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public OrderUserInfo User { get; set; } = null!;

    public IReadOnlyList<OrderItemInfo> PaintProducts { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<Order, OrderResult>> 
        Projection = o => new OrderResult
                                    {
                                        Id = o.Id,
                                        UserId = o.UserId,
                                        User = new OrderUserInfo ()
                                                            {
                                                                Id = o.User.Id,
                                                                Name = o.User.Name
                                                            },
                                        PaintProducts = o.OrderItems
                                                        .OrderBy(i=>i.Id)
                                                        .Select(i=>new OrderItemInfo{
                                                            PaintProductId = i.PaintProductId,
                                                            Name = i.PaintProduct.Name,
                                                            UnitPrice = i.UnitPrice,
                                                            Quantity = i.Quantity})
                                                        .ToList(),
                                        TotalPrice = o.TotalPrice,
                                        RowVersion = o.RowVersion
                                    };

    private static readonly Func<Order, OrderResult> Mapper = Projection.Compile();
    public static OrderResult FromEntity(Order order) => Mapper(order);
}
