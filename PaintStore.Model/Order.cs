using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace PaintStore.Models;

public class Order
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private init; }

    public int UserId { get; private init; }

    public User User { get; private init; } = null!;

    private readonly List<PaintProduct> _paintProducts = [];
    public IReadOnlyList<PaintProduct> PaintProducts => _paintProducts;

    public decimal TotalPrice => PaintProducts.Sum(p=>p.Price);

    private Order()
    {

    }

    public Order(int userId, User user, List<PaintProduct> paintProducts)
    {
        ArgumentNullException.ThrowIfNull(paintProducts);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 0);

        _paintProducts = paintProducts.ToList();
        UserId = userId;
        User = user;
        CreatedAt = DateTime.UtcNow;
    }
}
