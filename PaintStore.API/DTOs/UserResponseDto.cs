using System;

namespace PaintStore.API.DTOs;

public sealed class UserResponseDto
{
    public int Id {get; init;}
    public string Name { get; init;} = "";
    public string Email { get; init;} = "";
    public string Phone { get; init;} = "";
}
