using System;
using System.ComponentModel.DataAnnotations;

namespace PaintStore.API.DTOs;

public sealed class OrderItemCreateRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public required int PaintProductId { get; set;}

    [Required]
    [Range(1, int.MaxValue)]
    public required int Quantity { get; set; }
}
