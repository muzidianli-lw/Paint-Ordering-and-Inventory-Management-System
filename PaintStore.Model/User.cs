using System;

namespace PaintStore.Models;

public class User
{
    public int Id { get; }

    public string Name { get; } = "";

    public string Email { get; } = "";

    public string Phone { get; } = "";

    public DateTime CreatedAt { get; }

    public User(int id, string name, string email, string phone)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name is null or whitespace");
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("email is null or whitespace");
        }
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("phone is null or whitespace");
        }
    
        Id = id;
        Name = name.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
