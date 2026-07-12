using System;
using System.ComponentModel;

namespace paint_ordering_system.Models;

public class PaintSpecification
{
    public string Color { get; } = "";
    public int SizeInLiters { get; }

    public PaintSpecification(string color, int size)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Color is invalid", nameof(color));
        }
        if (size <= 0)
        {
            throw new ArgumentException("Size is invalid", nameof(size));
        }
        Color = color;
        SizeInLiters = size;
    }

    public void DisplaySpecification()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Size(L): {SizeInLiters}");
    }
}
