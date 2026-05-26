using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Pathfinding.Data;
using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.ViewModel.Requests;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Service.Algorithms.Events;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Extensions;
using Pathfinding.Service.Layers;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using static Pathfinding.Presentation.Console.Models.RunModel;
using static Pathfinding.Presentation.Console.ViewModels.ViewModel;
// ReSharper disable AccessToModifiedClosure

namespace Pathfinding.Presentation.Console.ViewModels;

[ViewModel]
internal sealed class RunFieldViewModel : ReactiveObject, IRunFieldViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IGraphAssemble<RunVertexModel> graphAssemble;
    private readonly IAlgorithmsFactory algorithmsFactory;

    private readonly CompositeDisposable disposables = [];
    private readonly CompositeDisposable shortTermDisposables = [];

    private ActiveGraph activeGraph;

    private RunModel selected = Empty;
    public RunModel SelectedRun
    {
        get => selected;
        set
        {
            float prevFraction = selected.Fraction;
            selected.Fraction = 0;
            this.RaiseAndSetIfChanged(ref selected, value);
            selected.Fraction = prevFraction;
        }
    }

    private ObservableCollection<RunModel> Runs { get; } = [];

    public IGraph<RunVertexModel> RunGraph { get; private set; } = Graph<RunVertexModel>.Empty;

    public RunFieldViewModel(
        IGraphAssemble<RunVertexModel> graphAssemble,
        IAlgorithmsFactory algorithmsFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger)
    {
        this.messenger = messenger;
        this.graphAssemble = graphAssemble;
        this.algorithmsFactory = algorithmsFactory;
        Runs.ActOnEveryObject(_ => { }, OnRemoved).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphUpdatedMessage>(this, OnGraphUpdated).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphActivatedMessage>(this, OnGraphActivated).DisposeWith(disposables);
        messenger.RegisterHandler<RunsDeletedMessage>(this, OnRunsDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterAsyncHandler<RunsSelectedMessage>(this, OnRunActivated).DisposeWith(disposables);
        shortTermDisposables.DisposeWith(disposables);
    }

    private async Task OnRunActivated(RunsSelectedMessage msg)
    {
        if (msg.Value.Length > 0)
        {
            var model = msg.Value[0];
            var run = Runs.FirstOrDefault(x => x.Id == model.Id);
            if (run == null)
            {
                this.RaisePropertyChanged(nameof(RunGraph));

                var range = messenger.SendAndGetResponse<PathfindingRangeRequestMessage, GraphVertexModel[]>(new());

                var subRevisions = new List<SubRunModel>();
                var visitedVertices = new List<VisitedModel>();

                void AddSubAlgorithm(IReadOnlyCollection<Coordinate> path = null)
                {
                    subRevisions.Add(new(visitedVertices, path ?? []));
                    visitedVertices = [];
                }
                void OnVertexProcessed(VerticesProcessedEventArgs e)
                {
                    visitedVertices.Add(new(e.Current, e.Enqueued));
                }
                void OnSubPathFound(SubPathFoundEventArgs args)
                {
                    AddSubAlgorithm(args.SubPath);
                }

                var factory = algorithmsFactory.Create(model.Algorithm);
                var algorithm = factory.Create(range, model);
                algorithm.SubPathFound += OnSubPathFound;
                algorithm.VertexProcessed += OnVertexProcessed;
                try
                {
                    await algorithm.FindPathAsync().ConfigureAwait(false);
                }
                catch
                {
                    AddSubAlgorithm();
                }
                finally
                {
                    algorithm.SubPathFound -= OnSubPathFound;
                    algorithm.VertexProcessed -= OnVertexProcessed;
                }

                var rangeCoordinates = range.Select(x => x.Position).ToArray();
                run = new(RunGraph, subRevisions, rangeCoordinates) { Id = model.Id };
                Runs.Add(run);
            }
            SelectedRun = run;
        }
    }

    private void OnRunsDeleted(RunsDeletedMessage msg)
    {
        if (msg.Value.Contains(SelectedRun.Id))
        {
            selected = Empty;
        }
        var runs = Runs.Where(x => msg.Value.Contains(x.Id)).ToArray();
        Runs.Remove(runs);
    }

    private Task OnGraphUpdated(AwaitGraphUpdatedMessage msg)
    {
        this.RaisePropertyChanged(nameof(RunGraph));
        Clear();
        return Task.CompletedTask;
    }

    private void Clear()
    {
        Runs.Clear();
        SelectedRun.Fraction = 0;
        selected = Empty;
        shortTermDisposables.Clear();
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(activeGraph.Id))
        {
            activeGraph = ActiveGraph.Empty;
            RunGraph = Graph<RunVertexModel>.Empty;
            this.RaisePropertyChanged(nameof(RunGraph));
            Clear();
        }
    }

    private async Task OnGraphActivated(AwaitGraphActivatedMessage msg)
    {
        activeGraph = msg.Value.ActiveGraph;
        var graphLayer = new GraphLayer(activeGraph.Graph);
        var runGraph = await graphAssemble.AssembleGraphAsync(graphLayer,
            activeGraph.Graph.DimensionsSizes);
        RunGraph = Graph<RunVertexModel>.Empty;
        this.RaisePropertyChanged(nameof(RunGraph));
        RunGraph = runGraph;
        Clear();
        foreach (var vertex in activeGraph.Graph)
        {
            var runVertex = runGraph.Get(vertex.Position);
            vertex.WhenAnyValue(x => x.IsObstacle)
                .BindTo(runVertex, x => x.IsObstacle)
                .DisposeWith(shortTermDisposables);
            vertex.WhenAnyValue(x => x.Cost)
                .Do(x => runVertex.Cost = x.DeepClone())
                .Subscribe()
                .DisposeWith(shortTermDisposables);
        }
    }

    private static void OnRemoved(RunModel model) => model.Dispose();

    public void Dispose()
    {
        disposables.Dispose();
    }
}