using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using PaintStore.Models;

namespace PaintStore.API.DTOs;

public sealed class OrderResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public OrderUserDto User { get; set; } = null!;

    public IReadOnlyList<OrderItemResponseDto> PaintProducts { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<Order, OrderResponseDto>> 
        Projection = o => new OrderResponseDto
                                    {
                                        Id = o.Id,
                                        UserId = o.UserId,
                                        User = new OrderUserDto ()
                                                            {
                                                                Id = o.User.Id,
                                                                Name = o.User.Name
                                                            },
                                        PaintProducts = o.OrderItems
                                                        .OrderBy(i=>i.Id)
                                                        .Select(i=>new OrderItemResponseDto{
                                                            PaintProductId = i.PaintProductId,
                                                            Name = i.PaintProduct.Name,
                                                            UnitPrice = i.UnitPrice,
                                                            Quantity = i.Quantity})
                                                        .ToList(),
                                        TotalPrice = o.TotalPrice,
                                        RowVersion = o.RowVersion
                                    };

    private static readonly Func<Order, OrderResponseDto> Mapper = Projection.Compile();
    public static OrderResponseDto FromEntity(Order order) => Mapper(order);
}
