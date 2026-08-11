using System;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace PaintStore.API.DTOs;

public sealed class OrderCreateRequestDto
{
    [Range(1, int.MaxValue)]
    public required int UserId { get; set; }

    [Required]
    [Length(1, int.MaxValue)]
    public List<int> PaintProductIds { get; set; } = [];
}
