using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Models;
using ReactiveUI;

namespace Pathfinding.App.Console.Models;

internal sealed class RunInfoModel : ReactiveObject, IAlgorithmBuildInfo
{
    public int Id { get; init; }

    public Algorithms Algorithm { get; init; }

    private int visited;
    public int Visited
    {
        get => visited;
        set => this.RaiseAndSetIfChanged(ref visited, value);
    }

    private int steps;
    public int Steps
    {
        get => steps;
        set => this.RaiseAndSetIfChanged(ref steps, value);
    }

    private double cost;
    public double Cost
    {
        get => cost;
        set => this.RaiseAndSetIfChanged(ref cost, value);
    }

    private TimeSpan elapsed;
    public TimeSpan Elapsed
    {
        get => elapsed;
        set => this.RaiseAndSetIfChanged(ref elapsed, value);
    }

    public StepRules? StepRule { get; init; }

    public Heuristics? Heuristics { get; init; }

    public double? Weight { get; init; }

    private RunStatuses status;
    public RunStatuses ResultStatus
    {
        get => status;
        set => this.RaiseAndSetIfChanged(ref status, value);
    }
}
