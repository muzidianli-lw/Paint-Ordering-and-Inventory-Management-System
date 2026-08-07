using System;
using System.Transactions;

namespace PaintStore.API.DTOs;

public sealed class UserResponseDto
{
    public int Id {get; init;}
    public string Name { get; init;} = "";
    public string Email { get; init;} = "";
    public string Phone { get; init;} = "";
    public byte[] RowVersion { get; set; } = null!;
}
