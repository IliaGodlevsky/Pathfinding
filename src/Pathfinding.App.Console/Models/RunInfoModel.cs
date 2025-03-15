using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Models;
using ReactiveUI;

namespace Pathfinding.App.Console.Models;

internal sealed class RunInfoModel : ReactiveObject, IAlgorithmBuildInfo
{
    public int Id { get; set; }

    public int GraphId { get; set; }

    public Algorithms Algorithm { get; set; }

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

    public StepRules? StepRule { get; set; }

    public Heuristics? Heuristics { get; set; }

    public double? Weight { get; set; }

    private RunStatuses status;
    public RunStatuses ResultStatus
    {
        get => status;
        set => this.RaiseAndSetIfChanged(ref status, value);
    }
}
