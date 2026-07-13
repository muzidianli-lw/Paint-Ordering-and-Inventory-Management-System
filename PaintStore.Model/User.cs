using System;
using System.ComponentModel.DataAnnotations;

namespace PaintStore.Models;

public class User
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string Name { get; init; } = "";

    [Required]
    public string Email { get; init; } = "";

    [Required]
    public string Phone { get; init; } = "";

    private readonly DateTime _createdDate;

    public User(int id, string name, string email)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        Id = id;
        Name = name.Trim();
        Email = email.Trim();
        _createdDate = DateTime.Now;
    }
}
