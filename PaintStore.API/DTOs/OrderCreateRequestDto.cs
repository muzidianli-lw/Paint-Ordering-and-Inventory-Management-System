using System;
using System.Transactions;

namespace PaintStore.API.DTOs;

public class OrderCreateRequestDto
{
    public int UserId { get; set; }
    public List<int> PaintProductIds { get; set; } = [];
}
