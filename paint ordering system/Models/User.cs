using System;

namespace paint_ordering_system.Models;

public class User
{
    private readonly List<Order> _orders = new();
    public IReadOnlyList<Order> Orders => _orders;

    private readonly List<Payment> _payments = new();
    public IReadOnlyList<Payment> Payments => _payments;

    public void AddOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _orders.Add(order);
    }

    public void AddPayment(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        _payments.Add(payment);
    }

    public Order GetMostExpensiveOrder()
    {
        if (_orders.Count == 0)
        {
            throw new InvalidOperationException("empty orderlist");
        }
        return _orders.MaxBy(o => o.GetTotalOrderPrice())!;
    }

    public Order GetNewestOrder()
    {
        if (_orders.Count == 0)
        {
            throw new InvalidOperationException("empty order list");
        }
        return _orders.MaxBy(o=>o.CreatedAt)!;
    }

    public Payment GetLowestPayment()
    {
        if (_payments.Count == 0)
        {
            throw new InvalidOperationException("empty payment list");
        }
        return _payments.MinBy(p=>p.PaymentAmount)!;
    }

    public Payment GetNewestPayment()
    {
        if (_payments.Count == 0)
        {
            throw new InvalidOperationException("empty payment list");
        }
        return _payments.MaxBy(p=>p.CreatedAt)!;
    }

    public List<Payment> GetPaymentsAbove(decimal threshold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(threshold, 0);
        return _payments.Where(p=>p.PaymentAmount > threshold).ToList();
    }
}
