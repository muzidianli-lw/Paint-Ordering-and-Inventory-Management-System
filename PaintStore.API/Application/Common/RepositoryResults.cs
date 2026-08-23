using System;
using PaintStore.API.Enums;

namespace PaintStore.API.Application.Common;

public class RepositoryResults<T>
{
    public T? Data { get; set; }
    public RepositoryResultsEnum ResultsEnum { get; set; }

}
