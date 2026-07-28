using System;
using PaintStore.Models;

namespace PaintStore.API.Data;

public static class MockData
{
    private static readonly List<Order> _orders = new(); 
    public static IReadOnlyList<Order> Orders => _orders;

    static MockData()
    {
        _orders.Add(new Order(1, 2, new List<PaintProduct>
            {
                new PaintProduct("abc", 100m),
                new PaintProduct("def", 75m)
            }
        ));

        _orders.Add(new Order(2, 3, new List<PaintProduct>
            {
                new PaintProduct("ghi", 125m),
                new PaintProduct("jkl", 50m)
            }
        ));
    }
}
