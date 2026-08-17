using System;
using System.Linq.Expressions;
using PaintStore.Models;

namespace PaintStore.API.Application.Users;

public class UserQueryResult
{
    public int Id {get; init;}
    public string Name { get; init;} = "";
    public string Email { get; init;} = "";
    public string Phone { get; init;} = "";
    public byte[] RowVersion { get; set; } = null!;

    public static readonly Expression<Func<User, UserQueryResult>> Projection = 
        u => new UserQueryResult
        {
            Id = u.Id,
            Name=u.Name, 
            Email=u.Email, 
            Phone=u.Phone,
            RowVersion=u.RowVersion
        };
    
    private static readonly Func<User, UserQueryResult> Mapper = Projection.Compile();
    public static UserQueryResult FromEntity(User user) => Mapper(user);
}
