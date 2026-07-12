using System;
using System.ComponentModel;
using paint_ordering_system.Enums;
using paint_ordering_system.Interfaces;

namespace paint_ordering_system.Models;

public class PaintProduct: IBuyable
{
    private readonly decimal _taxRate; //req
    private const decimal _defaultDiscount = 0.05m; //req

    public string Name { get; } = "";// 名字 req

    public PaintType Type { get; } // 类型 req

    public PaintSpecification Specification { get; } // 颜色 数量 req

    public Brand Brand { get; } // 品牌 req

    public decimal Price { get; } // 价格/升 req

    public PaintProduct(string name, PaintType type, PaintSpecification specification, decimal price, Brand brand) // req
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(brand);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual<decimal>(price, 0m);
        _taxRate = 0.1m;
        Name = name.Trim();
        Type = type;
        Specification = specification;
        Price = price;
        Brand = brand;
    }

    public decimal GetFinalPrice()  //req
    {
        decimal total = Specification.SizeInLiters * Price * (1 - _defaultDiscount) * (1 + _taxRate);
        return Math.Round(total, MidpointRounding.AwayFromZero);
    }

    public void DisplayInfo() //req
    {
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Type: {Type}");
        Brand.DisplayBrand();
        Specification.DisplaySpecification();
    }

    public decimal GetMaxDiscount(int rate, bool isOverridable) // req
    {
        if (isOverridable)
        {
            decimal inputDiscount = Math.Round(1/(decimal)rate, MidpointRounding.AwayFromZero);
            return Math.Max(inputDiscount, _defaultDiscount);
        }
        return _defaultDiscount;
    }
}
