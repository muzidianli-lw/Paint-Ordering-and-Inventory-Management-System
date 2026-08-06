using System;
using System.ComponentModel.DataAnnotations;
namespace PaintStore.API.DTOs;

public sealed class UserUpdateRequestDto
{
    [Required]
    public string Name { get; init;} = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init;} = null!;

    [Required]
    [Phone]
    public string Phone { get; init;} = null!;
}
