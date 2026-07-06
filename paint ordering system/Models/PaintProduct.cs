using System;
using System.ComponentModel;
using paint_ordering_system.Enums;
using paint_ordering_system.Interfaces;

namespace paint_ordering_system.Models;

public class PaintProduct: IBuyable
{
    private readonly decimal _taxRate;
    private const decimal _defaultDiscount = 0.05m;

    public string Name { get; set; }

    public PaintType Type { get; set; }

    public PaintSpecification Specification { get; set; }

    public decimal Price { get; set; }

    public PaintProduct(string name, PaintType type, PaintSpecification specification, decimal price)
    {
        _taxRate = 0.1m;
        Name = name;
        Type = type;
        Specification = specification;
        Price = price;
    }

    public decimal GetFinalPaice()
    {
        decimal total = Specification.SizeInLiters * Price * (1 - _defaultDiscount) * (1 + _taxRate);
        return Math.Round(total, MidpointRounding.AwayFromZero);
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Type: {Type}");
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
