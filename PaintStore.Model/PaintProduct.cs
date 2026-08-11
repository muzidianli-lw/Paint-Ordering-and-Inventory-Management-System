using System;

namespace PaintStore.Models;

public class PaintProduct
{
    public int Id { get; }
    public string Name { get; } = "";
    public decimal Price { get; }
    public DateTime CreatedAt {get; }

    public PaintProduct(int id, string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name is null or whitespace");
        }
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(price, 0);

        Id = id;
        Name = name.Trim();
        Price = price;
        CreatedAt = DateTime.UtcNow;
    }
}
