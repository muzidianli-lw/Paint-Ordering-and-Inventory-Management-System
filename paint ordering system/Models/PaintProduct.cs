using System;
using System.ComponentModel;
using paint_ordering_system.Enums;
using paint_ordering_system.Interfaces;

namespace paint_ordering_system.Models;

public class PaintProduct: IBuyable
{
    private readonly decimal _taxRate;
    private const decimal _defaultDiscount = 0.05m;

    public string Name { get; } = "";// 名字

    public PaintType Type { get; } // 类型

    public PaintSpecification Specification { get; } // 颜色 数量

    public Brand Brand { get; } // 品牌

    public decimal Price { get; private set; } // 价格/升

    public PaintProduct(string name, PaintType type, PaintSpecification specification, decimal price, Brand brand)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(brand);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0)
        {
            throw new ArgumentException("price is invalid");
        }
        _taxRate = 0.1m;
        Name = name.Trim();
        Type = type;
        Specification = specification;
        Price = price;
        Brand = brand;
    }

    public decimal GetFinalPrice()
    {
        decimal total = Specification.SizeInLiters * Price * (1 - _defaultDiscount) * (1 + _taxRate);
        return Math.Round(total, MidpointRounding.AwayFromZero);
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Type: {Type}");
        Brand.DisplayBrand();
        Specification.DisplaySpecification();
    }

    public decimal GetMaxDiscount(int rate, bool isOverridable)
    {
        if (isOverridable)
        {
            decimal inputDiscount = Math.Round(1/(decimal)rate, MidpointRounding.AwayFromZero);
            return Math.Max(inputDiscount, _defaultDiscount);
        }
        return _defaultDiscount;
    }
}
