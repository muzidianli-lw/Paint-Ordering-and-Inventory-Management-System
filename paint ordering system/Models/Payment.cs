using System;
using System.ComponentModel;
using paint_ordering_system.Enums;

namespace paint_ordering_system.Models;

public class Payment
{
    public int Id { get; init; }

    private PaymentState _paymentState;
    public PaymentState PaymentState
    { 
        get => _paymentState;
        set
        {
            if(!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _paymentState = value;
        }
    }

    private decimal _paymentAmount;
    public decimal PaymentAmount
    { 
        get => _paymentAmount;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0);
            _paymentAmount = value;  
        }
    }

    private PaymentMethod _paymentMethod;
    public PaymentMethod PaymentMethod
    { 
        get => _paymentMethod;
        set
        {
            if(!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _paymentMethod = value;
        }
    }

    public Payment(int id, PaymentState paymentState, decimal paymentAmount, PaymentMethod paymentMethod)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        Id = id;
        PaymentAmount = paymentAmount;
        PaymentMethod = paymentMethod;
        PaymentState = paymentState;
    }
}
