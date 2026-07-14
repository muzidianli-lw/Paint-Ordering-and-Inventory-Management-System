using System;

namespace PaintStore.Models;

public class Order
{
    public int Id { get; }
    public DateTime CreatedAt {get; }

    public int UserId { get; }
    
    private readonly List<PaintProduct> _paintProducts;
    public IReadOnlyList<PaintProduct> PaintProducts => _paintProducts;

    public decimal TotalPrice => PaintProducts.Sum(p=>p.Price);

    public Order(int id, int userId, List<PaintProduct> paintProducts)
    {
        ArgumentNullException.ThrowIfNull(paintProducts);
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 0);

        _paintProducts = paintProducts.ToList();
        Id = id;
        UserId = userId;
        CreatedAt = DateTime.Now;
    }
}
