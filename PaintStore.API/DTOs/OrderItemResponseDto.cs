using System;

namespace PaintStore.API.DTOs;

public class OrderItemResponseDto
{
    public int PaintProductId { get; set; }
    public string Name { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
