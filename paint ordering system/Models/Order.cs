using System;

namespace paint_ordering_system.Models;

public class Order
{
    private readonly DateTime _createdAt; // req

    private readonly List<OrderItem> _products = new();

    public IReadOnlyList<OrderItem> Products => _products;

    private decimal TotalPrice { get; }  //req

    public Order(List<OrderItem> products) // req
    {
        _products = products;
        _createdAt = DateTime.Now;
        TotalPrice = Math.Round(Products.Sum(p=>p.TotalPrice), 2, MidpointRounding.AwayFromZero);
    }

    public void DisplayOrder() // req
    {
        foreach (OrderItem product in Products)
        {
            product.DisplayOrderItem();
        }
    }

    public decimal GetTotalOrderPrice() // req
    {
        return TotalPrice;
    }
}
