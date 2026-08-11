using System;
using PaintStore.Models;

namespace PaintStore.API.DTOs;

public sealed class OrderResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public IReadOnlyList<PaintProduct> PaintProducts { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public OrderResponseDto()
    {
        
    }
    public OrderResponseDto(Order order)
    {
        Id = order.Id;
        UserId = order.UserId;
        User = order.User;
        PaintProducts = order.PaintProducts;
        TotalPrice = order.TotalPrice;
        RowVersion = order.RowVersion;
    }
}
