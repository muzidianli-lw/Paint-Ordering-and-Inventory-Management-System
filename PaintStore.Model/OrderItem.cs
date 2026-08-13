using System;

namespace PaintStore.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PaintProductId { get; init; }
    public PaintProduct PaintProduct { get; set; } = null!;
    public decimal UnitPrice { get; init; }
    private int _quantity;

    public int Quantity {
        get=>_quantity;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _quantity = value;
        }
    }
}
