using System;
using System.ComponentModel.DataAnnotations;

namespace PaintStore.Models;

public class PaintProduct
{
    [Required]
    public int Id { get; init; }
    [Required]
    public string Name { get; init; } = "";
    [Required]
    public decimal Price { get; init; }

    private readonly DateTime _createdDate;

    public PaintProduct(int id, string name, decimal price)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(price, 0);
        Id = id;
        Name = name;
        Price = price;
        _createdDate = DateTime.Now;
    }
}
