using Pathfinding.Domain.Core.Enums;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRunCreateViewModel
    {
        Algorithms? Algorithm { get; set; }

        IReadOnlyList<Algorithms> AllowedAlgorithms { get; }

        ReactiveCommand<Unit, Unit> CreateRunCommand { get; }
    }
}