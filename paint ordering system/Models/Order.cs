using System;

namespace paint_ordering_system.Models;

public class Order
{
    private readonly DateTime _createdAt;

    public List<OrderItem> Products { get; private set; } = new();

    private decimal _totalOrderPrice;

    public Order(List<OrderItem> products)
    {
        Products = products;
        _createdAt = DateTime.Now;
        _totalOrderPrice = Products.Sum(p=>p.TotalPrice);
    }

    public void DisplayOrder()
    {
        foreach (OrderItem product in Products)
        {
            product.DisplayOrderItem();
        }
    }

    public decimal GetTotalOrderPrice()
    {
        return Math.Round(_totalOrderPrice, 2, MidpointRounding.AwayFromZero);
    }
}
