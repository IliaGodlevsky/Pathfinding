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
        bool IsEnabled = true)
    {
        public void Apply() => Apply(IsEnabled);

        public void Misapply() => Apply(!IsEnabled);

        private void Apply(bool isEnabled)
        {
            switch (State)
            {
                case RunVertexState.Visited:
                    Vertex.IsVisited = isEnabled;
                    break;
                case RunVertexState.Enqueued:
                    Vertex.IsEnqueued = isEnabled;
                    break;
                case RunVertexState.CrossPath:
                    Vertex.IsCrossedPath = isEnabled;
                    break;
                case RunVertexState.Path:
                    Vertex.IsPath = isEnabled;
                    break;
                case RunVertexState.Source:
                    Vertex.IsSource = isEnabled;
                    break;
                case RunVertexState.Target:
                    Vertex.IsTarget = isEnabled;
                    break;
                case RunVertexState.Transit:
                    Vertex.IsTransit = isEnabled;
                    break;
            }
        }
    }

    public static readonly RunModel Empty = new(Graph<RunVertexModel>.Empty, [], []);
    private static readonly InclusiveValueRange<float> FractionRange = new(1);

    private readonly CompositeDisposable disposables = [];
    private readonly ReadOnlyCollection<RunVertexStateModel> verticesStates;
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
        verticesStates = GetVerticesStates(vertices,
            pathfindingResult, range);
        cursorRange = new(verticesStates.Count - 1);
        Bind(fraction => fraction > 0, Next);
        Bind(fraction => fraction < 0, Previous);
    }

    private void Bind(Func<int, bool> condition, Action<int> action)
    {
        this.WhenAnyValue(x => x.Fraction)
            .DistinctUntilChanged()
            .Select(x => (int)Math.Floor(verticesStates.Count * x - Cursor))
            .Where(condition)
            .Subscribe(action)
            .DisposeWith(disposables);
    }

    private void Next(int count)
    {
        while (count-- >= 0)
        {
            verticesStates[Cursor++].Apply();
        }
    }

    private void Previous(int count)
    {
        while (count++ <= 0)
        {
            verticesStates[Cursor--].Misapply();
        }
    }

    private static ReadOnlyCollection<RunVertexStateModel> GetVerticesStates(
        IGraph<RunVertexModel> graph,
        IReadOnlyCollection<SubRunModel> pathfindingResult,
        IReadOnlyCollection<Coordinate> range)
    {
        if (graph.Count == 0 || pathfindingResult.Count == 0 || range.Count < 2)
        {
            return ReadOnlyCollection<RunVertexStateModel>.Empty;
        }

        var visited = new HashSet<Coordinate>();
        var paths = new HashSet<Coordinate>();
        var enqueued = new HashSet<Coordinate>();
        var states = new List<RunVertexStateModel>();

        range.Skip(1).Take(range.Count - 2)
            .Select(x => ToRunVertexModel(x, RunVertexState.Transit))
            .Prepend(ToRunVertexModel(range.First(), RunVertexState.Source))
            .Append(ToRunVertexModel(range.Last(), RunVertexState.Target))
            .ForWhole(states.AddRange);

        foreach (var subAlgorithm in pathfindingResult)
        {
            var visitedIgnore = range.Concat(paths).ToArray();
            var exceptRangePath = subAlgorithm.Path.Except(range).ToArray();

            subAlgorithm.Visited.SelectMany(v =>
                 v.Enqueued.Intersect(visited).Except(visitedIgnore)
                    .Select(x => ToRunVertexModel(x, RunVertexState.Visited, false))
                    .Concat(v.Visited.Enumerate().Except(visitedIgnore)
                    .Select(x => ToRunVertexModel(x, RunVertexState.Visited))
                    .Concat(v.Enqueued.Except(visitedIgnore).Except(enqueued)
                    .Select(x => ToRunVertexModel(x, RunVertexState.Enqueued)))))
                    .Distinct().ForWhole(states.AddRange);

            exceptRangePath.Intersect(paths)
                .Select(x => ToRunVertexModel(x, RunVertexState.CrossPath))
                .Concat(exceptRangePath.Except(paths)
                .Select(x => ToRunVertexModel(x, RunVertexState.Path)))
                .ForWhole(states.AddRange);

            visited.AddRange(subAlgorithm.Visited.Select(x => x.Visited));
            enqueued.AddRange(subAlgorithm.Visited.SelectMany(x => x.Enqueued));
            paths.AddRange(subAlgorithm.Path);
        }

        return states.AsReadOnly();

        RunVertexStateModel ToRunVertexModel(Coordinate coordinate,
            RunVertexState stateType, bool isEnabled = true)
        {
            return new(graph.Get(coordinate), stateType, isEnabled);
        }
    }

    public void Dispose()
    {
        Fraction = 0;
        disposables.Dispose();
    }
}
