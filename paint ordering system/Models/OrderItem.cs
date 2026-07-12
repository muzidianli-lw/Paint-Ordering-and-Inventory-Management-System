using System;

namespace paint_ordering_system.Models;

public class OrderItem
{
    public PaintProduct Product { get; }

    public int Quantity { get; }

    public decimal TotalPrice { get; }

    public OrderItem(PaintProduct paintProduct, int quantity)
    {
        ArgumentNullException.ThrowIfNull(paintProduct);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual<int>(quantity, 0);

        Product = paintProduct;
        Quantity = quantity;
        TotalPrice = Math.Round(Quantity * Product.GetFinalPrice(), MidpointRounding.AwayFromZero);
    }

    public void DisplayOrderItem()
    {
        Product.DisplayInfo();
        Console.WriteLine($"Quantity: {Quantity}");
    }
}
