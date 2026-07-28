using System;

namespace PaintStore.Models;

public class PaintProduct
{
    public int Id { get; private set;}
    public string Name { get; private set;} = "";
    public decimal Price { get; private set;}
    public DateTime CreatedAt {get; private init;}

    private PaintProduct()
    {
        
    }

    public PaintProduct(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name is null or whitespace");
        }
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(price, 0);

        Name = name.Trim();
        Price = price;
        CreatedAt = DateTime.UtcNow;
    }
}
