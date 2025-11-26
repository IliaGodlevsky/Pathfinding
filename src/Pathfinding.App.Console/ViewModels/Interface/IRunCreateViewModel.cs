using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Core.Enums;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRunCreateViewModel
{
    IList<Algorithms> SelectedAlgorithms { get; }

    IReadOnlyList<Algorithms> AllowedAlgorithms { get; }

    IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }

    ReactiveCommand<Unit, Unit> CreateRunCommand { get; }
}