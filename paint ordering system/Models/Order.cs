using System;
using System.Dynamic;
using System.Net.Http.Headers;
using paint_ordering_system.Enums;

namespace paint_ordering_system.Models;

public class Order
{
    private readonly DateTime _createdAt; // req

    private readonly List<OrderItem> _products = new();

    public IReadOnlyList<OrderItem> Products => _products;

    private decimal TotalPrice=>Math.Round(Products.Sum(p=>p.TotalPrice), 2, MidpointRounding.AwayFromZero);

    public Order(List<OrderItem> products) // req
    {
        _products = products.ToList();
        _createdAt = DateTime.Now;
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

    public PaintProduct GetMostExpensivePaintProduct()
    {
        OrderItem? mostExpensiveOrderItm = _products.OrderBy(p=>p.TotalPrice).LastOrDefault();
        if (mostExpensiveOrderItm == null)
        {
            throw new InvalidOperationException("The order contain no products");
        }
        return mostExpensiveOrderItm.Product;
    }

    public bool RemoveProduct(int productId)
    {
        OrderItem? removedOrderItm = _products.FirstOrDefault(p=>p.Product.Id == productId);
        if(removedOrderItm == null)
        {
            return false;
        }
        return _products.Remove(removedOrderItm);
    }

    public List<OrderItem> GetPaintProductOfPriceBetween(decimal minPrice, decimal maxPrice)
    {
        if (minPrice >= maxPrice)
        {
            throw new ArgumentException("minPrice >= maxPrice");
        }
        return _products.Where(p=>(p.Product.GetFinalPrice() > minPrice) && (p.Product.GetFinalPrice() < maxPrice)).ToList();
    }

    public Dictionary<PaintType, decimal> GetTotalPriceForEachPaintType()
    {
        return _products.GroupBy(p=>p.Product.Type).ToDictionary(g=>g.Key, g=>g.Sum(p=>p.TotalPrice));
    }
}
