using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphTableViewModel : ViewModel, IGraphTableViewModel, IDisposable
{
    private readonly IGraphRequestService<GraphVertexModel> graphService;
    private readonly IGraphInfoRequestService graphInfoService;
    private readonly INeighborhoodLayerFactory neighborFactory;
    private readonly IMessenger messenger;
    private readonly CompositeDisposable disposables = [];

    public ReactiveCommand<Unit, Unit> LoadGraphsCommand { get; }

    public ReactiveCommand<int, Unit> ActivateGraphCommand { get; }

    public ReactiveCommand<int[], Unit> SelectGraphsCommand { get; }

    public ObservableCollection<GraphInfoModel> Graphs { get; } = [];

    private int ActivatedGraphId { get; set; }

    public GraphTableViewModel(
        IGraphRequestService<GraphVertexModel> graphService,
        IGraphInfoRequestService graphInfoService,
        INeighborhoodLayerFactory neighborFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger) : base(logger)
    {
        this.graphService = graphService;
        this.graphInfoService = graphInfoService;
        this.messenger = messenger;
        this.neighborFactory = neighborFactory;
        messenger.RegisterAwaitHandler<AwaitGraphUpdatedMessage>(this, OnGraphUpdated).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsCreatedMessage>(this, OnGraphCreated).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<ObstaclesCountChangedMessage>(this, OnObstaclesCountChanged).DisposeWith(disposables);
        messenger.RegisterHandler<GraphStateChangedMessage>(this, GraphStateChanged).DisposeWith(disposables);
        LoadGraphsCommand = ReactiveCommand.CreateFromTask(LoadGraphs).DisposeWith(disposables);
        ActivateGraphCommand = ReactiveCommand.CreateFromTask<int>(ActivatedGraph).DisposeWith(disposables);
        SelectGraphsCommand = ReactiveCommand.Create<int[]>(SelectGraphs).DisposeWith(disposables);
    }

    private void SelectGraphs(int[] selected)
    {
        var graphs = Graphs.Where(x => selected.Contains(x.Id)).ToArray();
        messenger.Send(new GraphsSelectedMessage(graphs));
    }

    private async Task ActivatedGraph(int model)
    {
        await ExecuteSafe(async token =>
        {
            var graphModel = await graphService
                .ReadGraphAsync(model, token)
                .ConfigureAwait(false);
            var graph = graphModel.CreateGraph();
            var activatedGraph = new ActiveGraph(
                graphModel.Id, 
                graph, 
                graphModel.Status == GraphStatuses.Readonly);
            var activated = new ActivatedGraphModel(activatedGraph,
                graphModel.Neighborhood,
                graphModel.SmoothLevel);
            var layer = neighborFactory.CreateNeighborhoodLayer(graphModel.Neighborhood);
            await layer
                .OverlayAsync(graph, token)
                .ConfigureAwait(false);
            await messenger.Send(new AwaitGraphActivatedMessage(activated));
            ActivatedGraphId = graphModel.Id;
        }).ConfigureAwait(false);
    }

    private async Task LoadGraphs()
    {
        await ExecuteSafe(async token =>
        {
            Graphs.Clear();
            var infos = await graphInfoService
                .ReadAllGraphInfoAsync(token)
                .ConfigureAwait(false);
            Graphs.Add(infos.ToGraphInfo());
        }).ConfigureAwait(false);
    }

    private void OnObstaclesCountChanged(ObstaclesCountChangedMessage msg)
    {
        var graph = Graphs.FirstOrDefault(x => x.Id == msg.Value.GraphId);
        if (graph != null)
        {
            graph.ObstaclesCount += msg.Value.Delta;
        }
    }

    private void GraphStateChanged(GraphStateChangedMessage msg)
    {
        var graph = Graphs.FirstOrDefault(x => x.Id == msg.Value.Id);
        if (graph != null)
        {
            graph.Status = msg.Value.Status;
        }
    }

    private async Task OnGraphUpdated(AwaitGraphUpdatedMessage msg)
    {
        var model = Graphs.FirstOrDefault(x => x.Id == msg.Value.Id);
        if (model != null)
        {
            model.Name = msg.Value.Name;
            var neighborhood = model.Neighborhood;
            if (model.Neighborhood != msg.Value.Neighborhood)
            {
                model.Neighborhood = msg.Value.Neighborhood;
                if (ActivatedGraphId == model.Id)
                {
                    await ActivatedGraph(ActivatedGraphId).ConfigureAwait(false);
                }
                await messenger.Send(new AwaitGraphStructureUpdatedMessage(msg.Value));
            }
        }
    }

    private void OnGraphCreated(GraphsCreatedMessage msg)
    {
        Graphs.Add(msg.Value);
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        var graphs = Graphs
            .Where(x => msg.Value.Contains(x.Id))
            .ToList();
        Graphs.Remove(graphs);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
