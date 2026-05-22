using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRunCreateViewModel
{
    IList<Algorithms> SelectedAlgorithms { get; }

    IReadOnlyList<Algorithms> AvailableAlgorithms { get; }

    IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }

    ReactiveCommand<Unit, Unit> CreateRunCommand { get; }
}