using System;
using System.ComponentModel;

namespace paint_ordering_system.Models;

public class PaintSpecification
{
    public string Color { get; set; }
    public int SizeInLiters { get; set; }

    public PaintSpecification(string color, int size)
    {
        Color = color;
        SizeInLiters = size;
    }

    public void DisplaySpecification()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Size(L): {SizeInLiters}");
    }
}
