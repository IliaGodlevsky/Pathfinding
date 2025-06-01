using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphTableViewModel : BaseViewModel, IGraphTableViewModel
{
    private readonly IRequestService<GraphVertexModel> service;
    private readonly IMessenger messenger;
    private readonly ILog logger;

    public ReactiveCommand<Unit, Unit> LoadGraphsCommand { get; }

    public ReactiveCommand<int, Unit> ActivateGraphCommand { get; }

    public ReactiveCommand<int[], Unit> SelectGraphsCommand { get; }

    public ObservableCollection<GraphInfoModel> Graphs { get; } = [];

    private int ActivatedGraphId { get; set; }

    public GraphTableViewModel(
        IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger)
    {
        this.service = service;
        this.messenger = messenger;
        this.logger = logger;
        messenger.RegisterAwaitHandler<AwaitGraphUpdatedMessage, int>(this, Tokens.GraphTable, OnGraphUpdated);
        messenger.Register<GraphsCreatedMessage>(this, OnGraphCreated);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
        messenger.Register<ObstaclesCountChangedMessage>(this, OnObstaclesCountChanged);
        messenger.Register<GraphStateChangedMessage>(this, GraphStateChanged);
        LoadGraphsCommand = ReactiveCommand.CreateFromTask(LoadGraphs);
        ActivateGraphCommand = ReactiveCommand.CreateFromTask<int>(ActivatedGraph);
        SelectGraphsCommand = ReactiveCommand.Create<int[]>(SelectGraphs);
    }

    private void SelectGraphs(int[] selected)
    {
        var graphs = Graphs.Where(x => selected.Contains(x.Id)).ToArray();
        messenger.Send(new GraphsSelectedMessage(graphs));
    }

    private async Task ActivatedGraph(int model)
    {
        await ExecuteSafe(async () =>
        {
            var graphModel = await service.ReadGraphAsync(model).ConfigureAwait(false);
            var graph = new Graph<GraphVertexModel>(graphModel.Vertices, graphModel.DimensionSizes);
            var activated = new ActivatedGraphModel(graph,
                graphModel.Neighborhood,
                graphModel.SmoothLevel,
                graphModel.Status,
                graphModel.Id);
            await graphModel.Neighborhood.ToNeighborhoodLayer().OverlayAsync(graph).ConfigureAwait(false);
            messenger.Send(new GraphActivatedMessage(activated), Tokens.GraphField);
            await messenger.Send(new AwaitGraphActivatedMessage(activated), Tokens.RunsTable);
            await messenger.Send(new AwaitGraphActivatedMessage(activated), Tokens.PathfindingRange);
            messenger.Send(new GraphActivatedMessage(activated));
            ActivatedGraphId = graphModel.Id;
        }, logger.Error).ConfigureAwait(false);
    }

    private async Task LoadGraphs()
    {
        await ExecuteSafe(async () =>
        {
            Graphs.Clear();
            var infos = await service.ReadAllGraphInfoAsync().ConfigureAwait(false);
            Graphs.Add(infos.ToGraphInfo());
        }, logger.Error).ConfigureAwait(false);
    }

    private void OnObstaclesCountChanged(object recipient, ObstaclesCountChangedMessage msg)
    {
        var graph = Graphs.FirstOrDefault(x => x.Id == msg.Value.GraphId);
        if (graph != null)
        {
            graph.ObstaclesCount += msg.Value.Delta;
        }
    }

    private void GraphStateChanged(object recipient, GraphStateChangedMessage msg)
    {
        var graph = Graphs.FirstOrDefault(x => x.Id == msg.Value.Id);
        if (graph != null)
        {
            graph.Status = msg.Value.Status;
        }
    }

    private async Task OnGraphUpdated(object recipient, AwaitGraphUpdatedMessage msg)
    {
        var model = Graphs.FirstOrDefault(x => x.Id == msg.Value.Id);
        if (model != null)
        {
            model.Name = msg.Value.Name;
            model.Neighborhood = msg.Value.Neighborhood;
            model.SmoothLevel = msg.Value.SmoothLevel;
            if (ActivatedGraphId == model.Id)
            {
                await ActivatedGraph(ActivatedGraphId);
            }
        }
    }

    private void OnGraphCreated(object recipient, GraphsCreatedMessage msg)
    {
        Graphs.Add(msg.Value);
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        var graphs = Graphs
            .Where(x => msg.Value.Contains(x.Id))
            .ToList();
        Graphs.Remove(graphs);
    }
}
