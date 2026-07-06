using System;

namespace paint_ordering_system.Models;

public class Order
{
    private readonly DateTime _createdAt;

    public PaintProduct Product { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public Order(PaintProduct paintProduct, int quantity)
    {
        Product = paintProduct;
        Quantity = quantity;
        _createdAt = DateTime.Now;
        TotalPrice = GetTotalPrice();
    }

    public void DisplayOrder()
    {
        Product.DisplayInfo();
        Console.WriteLine($"Quantity: {Quantity}");
    }

    public decimal GetTotalPrice()
    {
        return Math.Round(Quantity * Product.GetFinalPaice(), MidpointRounding.AwayFromZero);
    }
}
