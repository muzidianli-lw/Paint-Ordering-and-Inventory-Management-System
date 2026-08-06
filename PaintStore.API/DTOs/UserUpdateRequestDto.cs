using System;
using System.ComponentModel.DataAnnotations;
namespace PaintStore.API.DTOs;

public sealed class UserUpdateRequestDto
{
    [Required]
    [MaxLength(256)]
    public string Name { get; init;} = null!;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init;} = null!;

    [Required]
    [Phone]
    [MaxLength(32)]
    public string Phone { get; init;} = null!;
}
