using System;
using System.Text.Json.Serialization;

namespace PaintStore.Models;

public class User
{
    public int Id { get; private set;}

    public string Name { get; private set;} = "";

    public string Email { get; private set;} = "";

    public string Phone { get; private set;} = "";

    public DateTime CreatedAt { get; private init;}

    private User()
    {
        
    }

    public User(string name, string email, string phone)
    {
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
    
        Name = name.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
