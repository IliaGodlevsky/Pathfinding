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
        No,
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
        bool Value)
    {
        public static RunVertexStateModel CreateVisited(RunVertexModel model, bool value = true) => new(model, RunVertexState.Visited, value);

        public static RunVertexStateModel CreateEnqueued(RunVertexModel model) => new(model, RunVertexState.Enqueued, true);

        public static RunVertexStateModel CreateCrossedPath(RunVertexModel model) => new(model, RunVertexState.CrossPath, true);

        public static RunVertexStateModel CreatePath(RunVertexModel model) => new(model, RunVertexState.Path, true);

        public static RunVertexStateModel CreateSource(RunVertexModel model) => new(model, RunVertexState.Source, true);

        public static RunVertexStateModel CreateTarget(RunVertexModel model) => new(model, RunVertexState.Target, true);

        public static RunVertexStateModel CreateTransit(RunVertexModel model) => new(model, RunVertexState.Transit, true);

        public void Set() => Set(Value);

        public void SetInverse() => Set(!Value);

        private void Set(bool value)
        {
            switch (State)
            {
                case RunVertexState.Visited: Vertex.IsVisited = value; break;
                case RunVertexState.Enqueued: Vertex.IsEnqueued = value; break;
                case RunVertexState.CrossPath: Vertex.IsCrossedPath = value; break;
                case RunVertexState.Path: Vertex.IsPath = value; break;
                case RunVertexState.Source: Vertex.IsSource = value; break;
                case RunVertexState.Target: Vertex.IsTarget = value; break;
                case RunVertexState.Transit: Vertex.IsTransit = value; break;
            }
        }
    }

    public static readonly RunModel Empty = new(Graph<RunVertexModel>.Empty, [], []);

    private static readonly InclusiveValueRange<float> FractionRange = new(1, 0);

    private readonly CompositeDisposable disposables = [];
    private readonly Lazy<ReadOnlyCollection<RunVertexStateModel>> algorithm;
    private readonly Lazy<InclusiveValueRange<int>> cursorRange;

    private ReadOnlyCollection<RunVertexStateModel> Algorithm => algorithm.Value;

    private InclusiveValueRange<int> CursorRange => cursorRange.Value;

    public int Id { get; set; }

    public int Count => Algorithm.Count;

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
        set => this.RaiseAndSetIfChanged(ref cursor, CursorRange.ReturnInRange(value));
    }

    public RunModel(
        IGraph<RunVertexModel> vertices,
        IReadOnlyCollection<SubRunModel> pathfindingResult,
        IReadOnlyCollection<Coordinate> range)
    {
        algorithm = new(() => CreateAlgorithmRevision(vertices, pathfindingResult, range));
        cursorRange = new(() => new InclusiveValueRange<int>(Count - 1, 0));
        this.WhenAnyValue(x => x.Fraction)
            .DistinctUntilChanged().Select(GetCount)
            .Where(x => x > 0).Do(Next)
            .Subscribe().DisposeWith(disposables);
        this.WhenAnyValue(x => x.Fraction)
            .DistinctUntilChanged().Select(GetCount)
            .Where(x => x < 0).Do(Previous)
            .Subscribe().DisposeWith(disposables);
    }

    private int GetCount(float fraction)
        => (int)Math.Floor(Count * fraction - Cursor);

    private void Next(int count)
    {
        while (count-- >= 0) Algorithm[Cursor++].Set();
    }

    private void Previous(int count)
    {
        while (count++ <= 0) Algorithm[Cursor--].SetInverse();
    }

    private static ReadOnlyCollection<RunVertexStateModel> CreateAlgorithmRevision(
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
            .Select(x => RunVertexStateModel.CreateTransit(graph.Get(x)))
            .Prepend(RunVertexStateModel.CreateSource(graph.Get(range.First())))
            .Append(RunVertexStateModel.CreateTarget(graph.Get(range.Last())))
            .ForWhole(subAlgorithms.AddRange);

        foreach (var subAlgorithm in pathfindingResult)
        {
            var visitedIgnore = range.Concat(previousPaths).ToArray();
            var exceptRangePath = subAlgorithm.Path.Except(range).ToArray();

            subAlgorithm.Visited.SelectMany(v =>
                 v.Enqueued.Intersect(previousVisited).Except(visitedIgnore)
                    .Select(x => RunVertexStateModel.CreateVisited(graph.Get(x), false))
                    .Concat(v.Visited.Enumerate().Except(visitedIgnore)
                    .Select(x => RunVertexStateModel.CreateVisited(graph.Get(x)))
                    .Concat(v.Enqueued.Except(visitedIgnore).Except(previousEnqueued)
                    .Select(x => RunVertexStateModel.CreateEnqueued(graph.Get(x))))))
                    .Distinct().ForWhole(subAlgorithms.AddRange);

            exceptRangePath.Intersect(previousPaths)
                .Select(x => RunVertexStateModel.CreateCrossedPath(graph.Get(x)))
                .Concat(exceptRangePath.Except(previousPaths)
                .Select(x => RunVertexStateModel.CreatePath(graph.Get(x))))
                .ForWhole(subAlgorithms.AddRange);

            previousVisited.AddRange(subAlgorithm.Visited.Select(x => x.Visited));
            previousEnqueued.AddRange(subAlgorithm.Visited.SelectMany(x => x.Enqueued));
            previousPaths.AddRange(subAlgorithm.Path);
        }

        return subAlgorithms.AsReadOnly();
    }

    public void Dispose()
    {
        Fraction = 0;
        disposables.Dispose();
    }
}
