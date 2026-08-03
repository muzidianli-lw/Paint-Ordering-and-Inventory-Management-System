using System;
using System.ComponentModel.DataAnnotations;
namespace PaintStore.API.DTOs;

public class UserUpdateRequestDto
{
    [Required]
    public string Name { get; set;} = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set;} = null!;

    [Required]
    [Phone]
    public string Phone { get; set;} = null!;
}
