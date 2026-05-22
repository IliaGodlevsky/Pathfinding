using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Reactive;
using static Pathfinding.Presentation.Console.ViewModels.ViewModel;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IGraphFieldViewModel
{
    ActiveGraph ActivatedGraph { get; }

    ReactiveCommand<GraphVertexModel, Unit> ChangeVertexPolarityCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> DecreaseVertexCostCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> IncreaseVertexCostCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> ReverseVertexCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> InverseVertexCommand { get; }
}