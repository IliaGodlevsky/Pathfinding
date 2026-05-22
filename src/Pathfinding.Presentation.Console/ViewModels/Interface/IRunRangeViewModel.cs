using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRunRangeViewModel
{
    ReactiveCommand<GraphVertexModel, Unit> AddToRangeCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> RemoveFromRangeCommand { get; }

    ReactiveCommand<Unit, Unit> DeletePathfindingRange { get; }
}