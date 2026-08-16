using System;
using PaintStore.API.Enums;

namespace PaintStore.API.Services.Results;

public class ServiceResult<T>
{
    public ServiceResultsEnum State { get; set; }
    public string ErrorMsg { get; set; } = "";
    public T? Data { get; set; }
}
