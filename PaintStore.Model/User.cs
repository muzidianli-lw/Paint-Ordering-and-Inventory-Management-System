using System.ComponentModel.DataAnnotations;

namespace PaintStore.Models;

public class User
{
    public int Id { get; private set;}

    [MaxLength(256)]
    public string Name { get; private set;} = "";

    [MaxLength(256)]
    public string Email { get; private set;} = "";
    
    [MaxLength(32)]
    public string Phone { get; private set;} = "";

    public DateTime CreatedAt { get; private init;}

    public byte[] RowVersion { get; private set; } = null!;

    private User()
    {
        
    }

    private void UpdateInfo(string name, string email, string phone)
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
    }

    public User(string name, string email, string phone)
    {
        UpdateInfo(name, email, phone);
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string email, string phone)
    {
        UpdateInfo(name, email, phone);
    }
}
