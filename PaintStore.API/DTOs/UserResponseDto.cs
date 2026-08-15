using System;
using System.Linq.Expressions;
using System.Transactions;
using PaintStore.Models;

namespace PaintStore.API.DTOs;

public sealed class UserResponseDto
{
    public int Id {get; init;}
    public string Name { get; init;} = "";
    public string Email { get; init;} = "";
    public string Phone { get; init;} = "";
    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<User, UserResponseDto>> Projection = 
        u => new UserResponseDto
        {
            Id = u.Id,
            Name=u.Name, 
            Email=u.Email, 
            Phone=u.Phone,
            RowVersion=u.RowVersion
        };
    
    private static readonly Func<User, UserResponseDto> Mapper = Projection.Compile();
    public static UserResponseDto FromEntity(User user) => Mapper(user);

}
