namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface ICanPaginate
{
    int CurrentPage { get; set; }

    int PageSize { get; }

    int TotalPages { get; }
}
