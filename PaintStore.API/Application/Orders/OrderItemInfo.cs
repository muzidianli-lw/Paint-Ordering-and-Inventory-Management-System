using System;

namespace PaintStore.API.Application.Orders;

public class OrderItemInfo
{
    public int PaintProductId { get; set; }
    public string Name { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
