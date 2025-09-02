using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Models;

internal class RunModel : ReactiveObject, IDisposable
{
    public readonly record struct VisitedModel(
        Coordinate Visited,
        IReadOnlyList<Coordinate> Enqueued);

    public readonly record struct SubRunModel(
        IReadOnlyCollection<VisitedModel> Visited,
        IReadOnlyCollection<Coordinate> Path);

    private enum RunVertexState
    {
        Source,
        Target,
        Transit,
        Visited,
        Enqueued,
        Path,
        CrossPath
    }

    private readonly record struct RunVertexStateModel(
        RunVertexModel Vertex,
        RunVertexState State,
        bool Value = true)
    {
        public void Activate() => Activate(Value);

        public void Deactivate() => Activate(!Value);

        private void Activate(bool value)
        {
            switch (State)
            {
                case RunVertexState.Visited:
                    Vertex.IsVisited = value; 
                    break;
                case RunVertexState.Enqueued:
                    Vertex.IsEnqueued = value;
                    break;
                case RunVertexState.CrossPath: 
                    Vertex.IsCrossedPath = value; 
                    break;
                case RunVertexState.Path:
                    Vertex.IsPath = value;
                    break;
                case RunVertexState.Source:
                    Vertex.IsSource = value;
                    break;
                case RunVertexState.Target: 
                    Vertex.IsTarget = value;
                    break;
                case RunVertexState.Transit:
                    Vertex.IsTransit = value;
                    break;
            }
        }
    }

    public static readonly RunModel Empty = new(Graph<RunVertexModel>.Empty, [], []);
    private static readonly InclusiveValueRange<float> FractionRange = new(1);

    private readonly CompositeDisposable disposables = [];
    private readonly ReadOnlyCollection<RunVertexStateModel> algorithm;
    private readonly InclusiveValueRange<int> cursorRange;

    public int Id { get; init; }

    private float fraction;
    public float Fraction
    {
        get => fraction;
        set => this.RaiseAndSetIfChanged(ref fraction, FractionRange.ReturnInRange(value));
    }

    private int cursor;
    private int Cursor
    {
        get => cursor;
        set => cursor = cursorRange.ReturnInRange(value);
    }

    public RunModel(
        IGraph<RunVertexModel> vertices,
        IReadOnlyCollection<SubRunModel> pathfindingResult,
        IReadOnlyCollection<Coordinate> range)
    {
        algorithm = GetAlgorithmStates(vertices, 
            pathfindingResult, range);
        cursorRange = new(algorithm.Count - 1);
        Bind(fraction => fraction > 0, Next);
        Bind(fraction => fraction < 0, Previous);
    }

    private void Bind(Func<int, bool> condition, Action<int> action)
    {
        this.WhenAnyValue(x => x.Fraction)
            .DistinctUntilChanged()
            .Select(x => (int)Math.Floor(algorithm.Count * x - Cursor))
            .Where(condition)
            .Subscribe(action)
            .DisposeWith(disposables);
    }

    private void Next(int count)
    {
        while (count-- >= 0)
        {
            algorithm[Cursor++].Activate();
        }
    }

    private void Previous(int count)
    {
        while (count++ <= 0)
        {
            algorithm[Cursor--].Deactivate();
        }
    }

    private static ReadOnlyCollection<RunVertexStateModel> GetAlgorithmStates(
        IGraph<RunVertexModel> graph,
        IReadOnlyCollection<SubRunModel> pathfindingResult,
        IReadOnlyCollection<Coordinate> range)
    {
        if (graph.Count == 0 || pathfindingResult.Count == 0 || range.Count < 2)
        {
            return ReadOnlyCollection<RunVertexStateModel>.Empty;
        }

        var previousVisited = new HashSet<Coordinate>();
        var previousPaths = new HashSet<Coordinate>();
        var previousEnqueued = new HashSet<Coordinate>();
        var subAlgorithms = new List<RunVertexStateModel>();

        range.Skip(1).Take(range.Count - 2)
            .Select(x => ToRunVertexModel(x, RunVertexState.Transit))
            .Prepend(ToRunVertexModel(range.First(), RunVertexState.Source))
            .Append(ToRunVertexModel(range.Last(), RunVertexState.Target))
            .ForWhole(subAlgorithms.AddRange);

        foreach (var subAlgorithm in pathfindingResult)
        {
            var visitedIgnore = range.Concat(previousPaths).ToArray();
            var exceptRangePath = subAlgorithm.Path.Except(range).ToArray();

            subAlgorithm.Visited.SelectMany(v =>
                 v.Enqueued.Intersect(previousVisited).Except(visitedIgnore)
                    .Select(x => ToRunVertexModel(x, RunVertexState.Visited, false))
                    .Concat(v.Visited.Enumerate().Except(visitedIgnore)
                    .Select(x => ToRunVertexModel(x, RunVertexState.Visited))
                    .Concat(v.Enqueued.Except(visitedIgnore).Except(previousEnqueued)
                    .Select(x => ToRunVertexModel(x, RunVertexState.Enqueued)))))
                    .Distinct().ForWhole(subAlgorithms.AddRange);

            exceptRangePath.Intersect(previousPaths)
                .Select(x => ToRunVertexModel(x, RunVertexState.CrossPath))
                .Concat(exceptRangePath.Except(previousPaths)
                .Select(x => ToRunVertexModel(x, RunVertexState.Path)))
                .ForWhole(subAlgorithms.AddRange);

            previousVisited.AddRange(subAlgorithm.Visited.Select(x => x.Visited));
            previousEnqueued.AddRange(subAlgorithm.Visited.SelectMany(x => x.Enqueued));
            previousPaths.AddRange(subAlgorithm.Path);
        }

        return subAlgorithms.AsReadOnly();

        RunVertexStateModel ToRunVertexModel(Coordinate coordinate,
            RunVertexState stateType, bool state = true)
        {
            return new(graph.Get(coordinate), stateType, state);
        }
    }

    public void Dispose()
    {
        Fraction = 0;
        disposables.Dispose();
    }
}
