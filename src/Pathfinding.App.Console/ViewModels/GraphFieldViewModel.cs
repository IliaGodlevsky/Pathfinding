using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
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
using System.Reactive.Disposables;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphFieldViewModel : BaseViewModel, IGraphFieldViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly CompositeDisposable disposables = [];

    private int graphId;
    private int GraphId
    {
        get => graphId;
        set => this.RaiseAndSetIfChanged(ref graphId, value);
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
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.service = service;
        messenger.RegisterHandler<GraphActivatedMessage, int>(this, Tokens.GraphField, OnGraphActivated).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<GraphStateChangedMessage>(this, OnGraphBecameReadonly).DisposeWith(disposables);
        ChangeVertexPolarityCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(ChangePolarity, CanExecute()).DisposeWith(disposables);
        InverseVertexCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(InverseVertex, CanExecute()).DisposeWith(disposables);
        ReverseVertexCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(ReverseVertex, CanExecute()).DisposeWith(disposables);
        IncreaseVertexCostCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(IncreaseVertexCost, CanChangeCost()).DisposeWith(disposables);
        DecreaseVertexCostCommand = ReactiveCommand.CreateFromTask<GraphVertexModel>(DecreaseVertexCost, CanChangeCost()).DisposeWith(disposables);
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(
            x => x.GraphId,
            x => x.Graph,
            x => x.IsReadOnly,
            (id, x, isRead) => id > 0
                && x != Graph<GraphVertexModel>.Empty
                && !isRead);
    }

    private IObservable<bool> CanChangeCost()
    {
        return this.WhenAnyValue(
            x => x.GraphId,
            x => x.Graph,
            x => x.IsReadOnly,
            (id, x, isRead) => id > 0
                && x != Graph<GraphVertexModel>.Empty
                && !isRead);
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
            var inRangeRequest = new IsVertexInRangeRequestMessage(vertex);
            messenger.Send(inRangeRequest);
            if (!inRangeRequest.Response)
            {
                vertex.IsObstacle = polarity;
                messenger.Send(new ObstaclesCountChangedMessage((graphId, vertex.IsObstacle ? 1 : -1)));
                var request = new UpdateVerticesRequest<GraphVertexModel>(graphId,
                    [.. vertex.Enumerate()]);
                await ExecuteSafe(async token =>
                {
                    await service.UpdateVerticesAsync(request, token).ConfigureAwait(false);
                }).ConfigureAwait(false);
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
        await ExecuteSafe(async token =>
        {
            var cost = vertex.Cost.CurrentCost;
            cost += delta;
            cost = vertex.Cost.CostRange.ReturnInRange(cost);
            vertex.Cost = new VertexCost(cost, vertex.Cost.CostRange);
            var request = new UpdateVerticesRequest<GraphVertexModel>(GraphId, [.. vertex.Enumerate()]);
            await service.UpdateVerticesAsync(request, token).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private void OnGraphActivated(GraphActivatedMessage msg)
    {
        Graph = msg.Value.Graph;
        GraphId = msg.Value.GraphId;
        IsReadOnly = msg.Value.Status == GraphStatuses.Readonly;
    }

    private void OnGraphBecameReadonly(GraphStateChangedMessage msg)
    {
        IsReadOnly = msg.Value.Status == GraphStatuses.Readonly;
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(GraphId))
        {
            GraphId = 0;
            Graph = Graph<GraphVertexModel>.Empty;
            IsReadOnly = false;
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
