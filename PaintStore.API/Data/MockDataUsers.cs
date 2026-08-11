using System;
using PaintStore.Models;

namespace PaintStore.API.Data;

public class MockDataUsers
{
    public static List<User> Users = [
        new User(0, "abc", "abc@aaaa", "1300000"),
        new User(1, "def", "def@aaaa", "1312300"),
        new User(2, "ghi", "ghi@aaaa", "1300012"),
    ];
}
