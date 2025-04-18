using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Extensions;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphFieldViewModel : BaseViewModel, IGraphFieldViewModel
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly ILog logger;

    private int graphId;
    private int GraphId
    {
        get => graphId;
        set => this.RaiseAndSetIfChanged(ref graphId, value);
    }

    private SmoothLevels smoothLevels;
    private SmoothLevels SmoothLevel
    {
        get => smoothLevels;
        set => this.RaiseAndSetIfChanged(ref smoothLevels, value);
    }

    private bool isReadOnly;
    private bool IsReadOnly
    {
        get => isReadOnly;
        set => this.RaiseAndSetIfChanged(ref isReadOnly, value);
    }

    private IGraph<GraphVertexModel> graph = Graph<GraphVertexModel>.Empty;
    public IGraph<GraphVertexModel> Graph
    {
        get => graph;
        private set => this.RaiseAndSetIfChanged(ref graph, value);
    }

    public ReactiveCommand<GraphVertexModel, Unit> ChangeVertexPolarityCommand { get; }

    public ReactiveCommand<GraphVertexModel, Unit> ReverseVertexCommand { get; }

    public ReactiveCommand<GraphVertexModel, Unit> InverseVertexCommand { get; }

    public ReactiveCommand<GraphVertexModel, Unit> IncreaseVertexCostCommand { get; }

    public ReactiveCommand<GraphVertexModel, Unit> DecreaseVertexCostCommand { get; }

    public GraphFieldViewModel(
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        IRequestService<GraphVertexModel> service,
        ILog logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.logger = logger;
        messenger.Register<GraphActivatedMessage, int>(this, Tokens.GraphField, OnGraphActivated);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
        messenger.Register<GraphStateChangedMessage>(this, OnGraphBecameReadonly);
        var canExecute = CanExecute();
        var canChangeCost = CanChangeCost();
        ChangeVertexPolarityCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(ChangePolarity, canExecute);
        InverseVertexCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(InverseVertex, canExecute);
        ReverseVertexCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(ReverseVertex, canExecute);
        IncreaseVertexCostCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(IncreaseVertexCost, canChangeCost);
        DecreaseVertexCostCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(DecreaseVertexCost, canChangeCost);
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(
            x => x.GraphId,
            x => x.Graph,
            x => x.IsReadOnly,
            (id, graph, isRead) => id > 0
                && graph != Graph<GraphVertexModel>.Empty
                && !isRead);
    }

    private IObservable<bool> CanChangeCost()
    {
        return this.WhenAnyValue(
            x => x.GraphId,
            x => x.Graph,
            x => x.IsReadOnly,
            x => x.SmoothLevel,
            (id, graph, isRead, level) => id > 0
                && graph != Graph<GraphVertexModel>.Empty
                && !isRead
                && level == SmoothLevels.No);
    }

    private async Task ReverseVertex(GraphVertexModel vertex)
    {
        await ChangeVertexPolarity(vertex, true).ConfigureAwait(false);
    }

    private async Task InverseVertex(GraphVertexModel vertex)
    {
        await ChangeVertexPolarity(vertex, false).ConfigureAwait(false);
    }

    private async Task ChangePolarity(GraphVertexModel vertex)
    {
        await ChangeVertexPolarity(vertex, !vertex.IsObstacle).ConfigureAwait(false);
    }

    private async Task ChangeVertexPolarity(GraphVertexModel vertex, bool polarity)
    {
        if (vertex.IsObstacle != polarity)
        {
            var inRangeRquest = new IsVertexInRangeRequest(vertex);
            messenger.Send(inRangeRquest);
            if (!inRangeRquest.IsInRange)
            {
                vertex.IsObstacle = polarity;
                messenger.Send(new ObstaclesCountChangedMessage(graphId, vertex.IsObstacle ? 1 : -1));
                var request = new UpdateVerticesRequest<GraphVertexModel>(graphId,
                    vertex.Enumerate().ToList());
                await ExecuteSafe(async () =>
                {
                    await service.UpdateVerticesAsync(request).ConfigureAwait(false);
                }, logger.Error).ConfigureAwait(false);
            }
        }
    }

    private async Task IncreaseVertexCost(GraphVertexModel vertex)
    {
        await ChangeVertexCost(vertex, 1).ConfigureAwait(false);
    }

    private async Task DecreaseVertexCost(GraphVertexModel vertex)
    {
        await ChangeVertexCost(vertex, -1).ConfigureAwait(false);
    }

    private async Task ChangeVertexCost(GraphVertexModel vertex, int delta)
    {
        var cost = vertex.Cost.CurrentCost;
        cost += delta;
        cost = vertex.Cost.CostRange.ReturnInRange(cost);
        vertex.Cost = new VertexCost(cost, vertex.Cost.CostRange);
        var request = new UpdateVerticesRequest<GraphVertexModel>(GraphId, [.. vertex.Enumerate()]);
        await ExecuteSafe(async () => await service.UpdateVerticesAsync(request)
            .ConfigureAwait(false),
            logger.Error).ConfigureAwait(false);
    }

    private void OnGraphActivated(object recipient, GraphActivatedMessage msg)
    {
        Graph = new Graph<GraphVertexModel>(msg.Graph.Vertices, msg.Graph.DimensionSizes);
        GraphId = msg.Graph.Id;
        IsReadOnly = msg.Graph.Status == GraphStatuses.Readonly;
        SmoothLevel = msg.Graph.SmoothLevel;
    }

    private void OnGraphBecameReadonly(object recipient, GraphStateChangedMessage msg)
    {
        IsReadOnly = msg.Status == GraphStatuses.Readonly;
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        if (msg.GraphIds.Contains(GraphId))
        {
            GraphId = 0;
            Graph = Graph<GraphVertexModel>.Empty;
            IsReadOnly = false;
        }
    }
}
