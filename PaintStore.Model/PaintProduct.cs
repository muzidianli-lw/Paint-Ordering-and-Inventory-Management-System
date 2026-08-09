using System;

namespace PaintStore.Models;

public class PaintProduct
{
    public int Id { get; private set;}
    public string Name { get; private set;} = "";
    public string Brand { get; private set; } = "";
    public decimal Price { get; private set;}
    public int Inventory { get; private set; }
    public DateTime CreatedAt {get; private init;}

    public byte[] RowVersion { get; set; } = null!;

    private PaintProduct()
    {
        
    }

    public PaintProduct(string name, decimal price, string brand, int inventory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(brand);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(price, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(inventory, 1);

        Name = name.Trim();
        Brand = brand.Trim();
        Price = price;
        Inventory = inventory;
        CreatedAt = DateTime.UtcNow;
    }
}
