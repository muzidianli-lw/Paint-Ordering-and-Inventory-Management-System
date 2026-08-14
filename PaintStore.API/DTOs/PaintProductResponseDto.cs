using System;
using System.Linq.Expressions;
using PaintStore.Models;

namespace PaintStore.API.DTOs;

public class PaintProductResponseDto
{
    public int Id { get; set;}
    public string Name { get; set;} = null!;
    public string Brand { get; set; } = null!;
    public decimal Price { get; set;}
    public int Inventory { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<PaintProduct, PaintProductResponseDto>> Projection = 
                                                                            p=> new PaintProductResponseDto()
                                                                                {Id=p.Id,
                                                                                Name=p.Name,
                                                                                Brand=p.Brand,
                                                                                Price=p.Price,
                                                                                Inventory=p.Inventory,
                                                                                RowVersion=p.RowVersion
                                                                                };
    private static readonly Func<PaintProduct, PaintProductResponseDto> Mapper = Projection.Compile();
    public static PaintProductResponseDto FromEntity(PaintProduct paintProduct) => Mapper(paintProduct);
}
