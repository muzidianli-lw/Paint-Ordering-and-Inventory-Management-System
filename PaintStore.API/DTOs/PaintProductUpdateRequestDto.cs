using System;
using System.ComponentModel.DataAnnotations;

namespace PaintStore.API.DTOs;

public sealed class PaintProductUpdateRequestDto
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set;} = null!;
    [Required]
    [MaxLength(256)]
    public string Brand { get; set; } = null!;
    [Required]
    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Price { get; set;}
    [Required]
    [Range(1, int.MaxValue)]
    public int Inventory { get; set; }
    [Required]
    [MinLength(8)]
    [MaxLength(8)]
    public byte[] RowVersion { get; set; } = null!;
}