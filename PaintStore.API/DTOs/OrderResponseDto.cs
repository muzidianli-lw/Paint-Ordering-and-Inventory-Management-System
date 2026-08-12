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

    public IReadOnlyList<OrderPaintProductDto> PaintProducts { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<Order, OrderResponseDto>> Projection =
                                                o => new OrderResponseDto
                                                {
                                                    Id = o.Id,
                                                    UserId = o.UserId,
                                                    User = new OrderUserDto ()
                                                                        {
                                                                            Id = o.User.Id,
                                                                            Name = o.User.Name
                                                                        },
                                                    PaintProducts = o.PaintProducts
                                                                    .Select(p=>new OrderPaintProductDto{
                                                                        Id = p.Id,
                                                                        Name = p.Name,
                                                                        Brand = p.Brand,
                                                                        Price = p.Price})
                                                                    .ToList(),
                                                    TotalPrice = o.TotalPrice,
                                                    RowVersion = o.RowVersion
                                                };

    private static readonly Func<Order, OrderResponseDto> Mapper = Projection.Compile();
    public static OrderResponseDto FromEntity(Order order) => Mapper(order);
}
