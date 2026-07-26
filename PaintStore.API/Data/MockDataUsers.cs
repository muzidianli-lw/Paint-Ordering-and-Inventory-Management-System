using System;
using PaintStore.Models;

namespace PaintStore.API.Data;

public class MockDataUsers
{
    public static List<User> Users = [
        new User("abc", "abc@aaaa", "1300000"),
        new User("def", "def@aaaa", "1312300"),
        new User("ghi", "ghi@aaaa", "1300012"),
    ];
}
