using System;

namespace PaintStore.API.DTOs;
using System.ComponentModel.DataAnnotations;

public class OrderUpdateRequestDto
{
    [Required]
    [Length(1, int.MaxValue)]
    public List<OrderItemCreateRequestDto> OrderItems { get; set; } = [];

    [Required]
    [MinLength(8)]
    [MaxLength(8)]
    public byte[] RowVersion { get; set; } = null!;
}
