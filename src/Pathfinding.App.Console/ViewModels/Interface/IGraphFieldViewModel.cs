using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Interface;
using ReactiveUI;
using System.Reactive;
using static Pathfinding.App.Console.ViewModels.ViewModel;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IGraphFieldViewModel
{
    ActiveGraph ActivatedGraph { get; }

    ReactiveCommand<GraphVertexModel, Unit> ChangeVertexPolarityCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> DecreaseVertexCostCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> IncreaseVertexCostCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> ReverseVertexCommand { get; }

    ReactiveCommand<GraphVertexModel, Unit> InverseVertexCommand { get; }
}