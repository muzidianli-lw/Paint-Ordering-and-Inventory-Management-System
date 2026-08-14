using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace PaintStore.Models;

public class Order
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }
    public int UserId { get; init; }

    public User User { get; init; } = null!;

    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems;

    public decimal TotalPrice { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

    private Order()
    {

    }

    public void Update(List<OrderItem> items)
    {
        UpdateData(items);
    }

    private void UpdateData(List<OrderItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfEqual(items.Count, 0);
        foreach (OrderItem item in items)
        {
            ArgumentNullException.ThrowIfNull(item.PaintProduct);
            ArgumentOutOfRangeException.ThrowIfLessThan(item.PaintProductId, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(item.Quantity, 1);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(item.UnitPrice, 0);
        }
        Dictionary<int, OrderItem> oldOrderItems = _orderItems
                                            .ToDictionary(o=>o.PaintProductId, o=>o);
        Dictionary<int, OrderItem> newOrderItems = items
                                            .ToDictionary(o=>o.PaintProductId, o=>o);
        
        foreach (var pid in oldOrderItems.Keys)
        {
            if(newOrderItems.ContainsKey(pid))
            {
                if (newOrderItems[pid].Quantity > oldOrderItems[pid].Quantity)
                {
                    oldOrderItems[pid].UnitPrice = (oldOrderItems[pid].UnitPrice * oldOrderItems[pid].Quantity
                    + newOrderItems[pid].UnitPrice * (newOrderItems[pid].Quantity - oldOrderItems[pid].Quantity))/newOrderItems[pid].Quantity;
                }
                oldOrderItems[pid].Quantity = newOrderItems[pid].Quantity;
            }
            else
            {
                _orderItems.Remove(oldOrderItems[pid]);
            }
        }
        
        List<int> newId = newOrderItems.Keys.Where(k=>!oldOrderItems.ContainsKey(k)).ToList();
        foreach (var pid in newId)
        {
            _orderItems.Add(newOrderItems[pid]);
        }

        TotalPrice = _orderItems.Sum(p=>p.UnitPrice * p.Quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public Order(int userId, User user, List<OrderItem> items)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentOutOfRangeException.ThrowIfNotEqual(userId, user.Id);

        UserId = userId;
        User = user;
        CreatedAt = DateTime.UtcNow;

        UpdateData(items);
    }
}
