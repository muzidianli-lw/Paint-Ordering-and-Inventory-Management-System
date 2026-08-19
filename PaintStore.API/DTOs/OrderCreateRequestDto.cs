using System.ComponentModel.DataAnnotations;
using PaintStore.API.Application.Orders;

namespace PaintStore.API.DTOs;

public sealed class OrderCreateRequestDto
{
    [Range(1, int.MaxValue)]
    public required int UserId { get; set; }

    [Required]
    [Length(1, int.MaxValue)]
    public List<OrderItemCreateRequest> Items { get; set; } = [];
}
