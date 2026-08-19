using System;
using System.Linq.Expressions;
using PaintStore.Models;

namespace PaintStore.API.Application.Common;

public sealed class PaintProductResult
{
    public int Id { get; set;}
    public string Name { get; set;} = null!;
    public string Brand { get; set; } = null!;
    public decimal Price { get; set;}
    public int Inventory { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<PaintProduct, PaintProductResult>> Projection = 
                                                                            p=> new PaintProductResult()
                                                                                {Id=p.Id,
                                                                                Name=p.Name,
                                                                                Brand=p.Brand,
                                                                                Price=p.Price,
                                                                                Inventory=p.Inventory,
                                                                                RowVersion=p.RowVersion
                                                                                };
    private static readonly Func<PaintProduct, PaintProductResult> Mapper = Projection.Compile();
    public static PaintProductResult FromEntity(PaintProduct paintProduct) => Mapper(paintProduct);
}
