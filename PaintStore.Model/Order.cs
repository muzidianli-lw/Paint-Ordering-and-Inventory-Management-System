using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace PaintStore.Models;

public class Order
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private init; }

    public int UserId { get; init; }

    public User User { get; init; } = null!;

    private readonly List<PaintProduct> _paintProducts = [];
    public IReadOnlyList<PaintProduct> PaintProducts => _paintProducts;

    public decimal TotalPrice { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

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
        TotalPrice = _paintProducts.Sum(p=>p.Price);
    }
}
