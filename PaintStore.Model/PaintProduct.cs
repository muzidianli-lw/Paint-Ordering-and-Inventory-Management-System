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
    public byte[] RowVersion { get; private set; } = null!;

    private PaintProduct()
    {
        
    }

    /*
    return:
    -1 输入参数错误
    >=0 实际减少数量
    */
    public int UpdateInventory(int cnt)
    {
        int left = Inventory + cnt;
        if (left < 0)
        {
            return 0;
        }
        Inventory = left;
        return cnt;
    }

    public void Update(string name, decimal price, string brand, int inventory)
    {
        CheckAndUpdateData(name, price, brand, inventory);
    }

    public PaintProduct(string name, decimal price, string brand, int inventory)
    {
        CheckAndUpdateData(name, price, brand, inventory);
        CreatedAt = DateTime.UtcNow;
    }

    private void CheckAndUpdateData(string name, decimal price, string brand, int inventory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(brand);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(price, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(inventory, 1);

        Name = name.Trim();
        Brand = brand.Trim();
        Price = price;
        Inventory = inventory;
    }
}
