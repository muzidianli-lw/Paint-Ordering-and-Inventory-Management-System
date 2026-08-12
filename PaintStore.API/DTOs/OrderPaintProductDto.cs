using System;

namespace PaintStore.API.DTOs;

public class OrderPaintProductDto
{
    public int Id { get; set;}
    public string Name { get; set;} = null!;
    public string Brand { get; set; } = null!;
    public decimal Price { get; set;}
}
