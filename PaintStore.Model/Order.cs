using System;
using System.ComponentModel.DataAnnotations;

namespace PaintStore.Models;

public class Order
{
    [Required]
    public int Id { get; init; }
    public readonly DateTime _createdDate;

    [Required]
    public int UserId { get; init; }
    
    [Required]
    public List<PaintProduct> PaintProducts { get; set; } = new();

    public decimal Price => PaintProducts.Sum(p=>p.Price);

    public Order(int id, int userId, List<PaintProduct> paintProducts)
    {
        ArgumentNullException.ThrowIfNull(paintProducts);
        PaintProducts = paintProducts.ToList();
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 0);
        Id = id;
        UserId = userId;
        _createdDate = DateTime.Now;
    }

    public DateTime GetCreatedTime()
    {
        return _createdDate;
    }
}
