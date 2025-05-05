using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Undefined;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunUpdateViewModel : BaseViewModel, IRunUpdateViewModel
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly ILog log;

    private RunInfoModel[] selected = [];
    private RunInfoModel[] Selected
    {
        get => selected;
        set => this.RaiseAndSetIfChanged(ref selected, value);
    }

    private Graph<GraphVertexModel> graph = Graph<GraphVertexModel>.Empty;
    private Graph<GraphVertexModel> Graph
    {
        get => graph;
        set => this.RaiseAndSetIfChanged(ref graph, value);
    }

    private int ActivatedGraphId { get; set; }

    public ReactiveCommand<Unit, Unit> UpdateRunsCommand { get; }

    public RunUpdateViewModel(IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog log)
    {
        this.messenger = messenger;
        this.service = service;
        this.log = log;
        UpdateRunsCommand = ReactiveCommand.CreateFromTask(ExecuteUpdate, CanUpdate());
        messenger.Register<RunsSelectedMessage>(this, OnRunsSelected);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
        messenger.Register<GraphActivatedMessage>(this, OnGraphActivated);
        messenger.RegisterAsyncHandler<AsyncGraphUpdatedMessage, int>(this,
            Tokens.AlgorithmUpdate, OnGraphUpdated);
    }

    private void OnRunsSelected(object recipient, RunsSelectedMessage msg)
    {
        Selected = msg.Value;
    }

    private void OnGraphActivated(object recipient, GraphActivatedMessage msg)
    {
        Graph = msg.Value.Graph;
        ActivatedGraphId = msg.Value.GraphId;
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        if (Graph != null && msg.Value.Contains(ActivatedGraphId))
        {
            Selected = [];
            Graph = Graph<GraphVertexModel>.Empty;
            ActivatedGraphId = 0;
        }
    }

    private IObservable<bool> CanUpdate()
    {
        return this.WhenAnyValue(x => x.Selected,
            x => x.Graph, (s, g) => s.Length > 0 && g != Graph<GraphVertexModel>.Empty);
    }

    private async Task ExecuteUpdate()
    {
        await ExecuteSafe(async () =>
        {
            var models = await service.ReadStatisticsAsync(Selected.Select(x => x.Id))
                .ConfigureAwait(false);
            var updated = await UpdateRunsAsync(models, Graph, ActivatedGraphId).ConfigureAwait(false);
            messenger.Send(new RunsUpdatedMessage(updated));
        }, log.Error).ConfigureAwait(false);
    }

    private async Task OnGraphUpdated(object recipient, AsyncGraphUpdatedMessage msg)
    {
        var graph = Graph;
        int id = ActivatedGraphId;
        if ((graph != Graph<GraphVertexModel>.Empty && msg.Value.Id != ActivatedGraphId)
            || graph == Graph<GraphVertexModel>.Empty)
        {
            var model = await service.ReadGraphAsync(msg.Value.Id).ConfigureAwait(false);
            graph = new(model.Vertices, model.DimensionSizes);
            id = model.Id;
            await model.ToLayers().OverlayAsync(graph).ConfigureAwait(false);
        }
        if (graph != Graph<GraphVertexModel>.Empty)
        {
            await ExecuteSafe(async () =>
            {
                var models = await service.ReadStatisticsAsync(id).ConfigureAwait(false);
                var updated = await UpdateRunsAsync(models, graph, id);
                messenger.Send(new RunsUpdatedMessage(updated));
            }, log.Error).ConfigureAwait(false);
        }

        msg.SetCompleted(true);
    }

    private async Task<RunStatisticsModel[]> UpdateRunsAsync(
        IEnumerable<RunStatisticsModel> selected, Graph<GraphVertexModel> graph, int graphId)
    {
        var range = (await service.ReadRangeAsync(graphId).ConfigureAwait(false))
            .Select(x => graph.Get(x.Position))
            .ToList();
        var updatedRuns = new List<RunStatisticsModel>();
        if (range.Count > 1)
        {
            foreach (var select in selected)
            {
                int visitedCount = 0;
                void OnVertexProcessed(EventArgs e) => visitedCount++;
                var info = await service.ReadStatisticAsync(select.Id).ConfigureAwait(false);
                var algorithm = select.ToAlgorithm(range);
                algorithm.VertexProcessed += OnVertexProcessed;

                var status = RunStatuses.Success;
                var path = NullGraphPath.Interface;
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    path = algorithm.FindPath();
                }
                catch
                {
                    status = RunStatuses.Failure;
                }

                stopwatch.Stop();
                algorithm.VertexProcessed -= OnVertexProcessed;

                info.Elapsed = stopwatch.Elapsed;
                info.Visited = visitedCount;
                info.Cost = path.Cost;
                info.Steps = path.Count;
                info.ResultStatus = status;
                updatedRuns.Add(info);
            }
            await ExecuteSafe(async () =>
            {
                await service.UpdateStatisticsAsync(updatedRuns).ConfigureAwait(false);
            }, (ex, msg) =>
            {
                log.Error(ex, msg);
                updatedRuns.Clear();
            });
        }
        return [.. updatedRuns];
    }
}
