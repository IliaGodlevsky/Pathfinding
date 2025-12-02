using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Algorithms.Events;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using static Pathfinding.App.Console.Models.RunModel;
using static Pathfinding.App.Console.ViewModels.ViewModel;
// ReSharper disable AccessToModifiedClosure

namespace Pathfinding.App.Console.ViewModels;

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
        messenger.RegisterHandler<RunsSelectedMessage>(this, OnRunActivated).DisposeWith(disposables);
        shortTermDisposables.DisposeWith(disposables);
    }

    private void OnRunActivated(RunsSelectedMessage msg)
    {
        if (msg.Value.Length > 0)
        {
            ActivateRun(msg.Value[0]);
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

    private Task OnGraphActivated(AwaitGraphActivatedMessage msg)
    {
        activeGraph = msg.Value.ActiveGraph;
        var runGraph = graphAssemble.AssembleGraph(activeGraph.Graph,
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
        return Task.CompletedTask;
    }

    private static void OnRemoved(RunModel model) => model.Dispose();

    private void ActivateRun(RunInfoModel model)
    {
        var run = Runs.FirstOrDefault(x => x.Id == model.Id);
        if (run == null)
        {
            this.RaisePropertyChanged(nameof(RunGraph));

            var rangeMsg = new PathfindingRangeRequestMessage();
            messenger.Send(rangeMsg);

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

            var factory = algorithmsFactory.GetAlgorithmFactory(model.Algorithm);
            var algorithm = factory.CreateAlgorithm(rangeMsg.Response, model);
            algorithm.SubPathFound += OnSubPathFound;
            algorithm.VertexProcessed += OnVertexProcessed;
            try
            {
                algorithm.FindPath();
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

            var rangeCoordinates = rangeMsg.Response.Select(x => x.Position).ToArray();
            run = new(RunGraph, subRevisions, rangeCoordinates) { Id = model.Id };
            Runs.Add(run);
        }
        SelectedRun = run;
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}