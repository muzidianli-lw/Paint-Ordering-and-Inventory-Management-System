using System;

namespace paint_ordering_system.Models;

public class Brand
{
    public string Name { get;}

    public string Discription { get;}

    public Brand(string name, string discription)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name is invalid");
        }
        Name = name.Trim();
        Discription = discription ?? "";
    }

    public void DisplayBrand()
    {
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Discription: {Discription}");
    }
}
