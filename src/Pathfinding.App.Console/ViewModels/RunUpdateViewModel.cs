using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Factories;
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
using System.Reactive.Disposables;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunUpdateViewModel : ViewModel, IRunUpdateViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IGraphRequestService<GraphVertexModel> graphService;
    private readonly IRangeRequestService<GraphVertexModel> rangeService;
    private readonly IStatisticsRequestService statisticsService;
    private readonly IAlgorithmsFactory algorithmsFactory;
    private readonly INeighborhoodLayerFactory neighborFactory;
    private readonly CompositeDisposable disposables = [];

    private RunInfoModel[] selected = [];
    private RunInfoModel[] Selected
    {
        get => selected;
        set => this.RaiseAndSetIfChanged(ref selected, value);
    }

    private ActiveGraph activatedGraph;
    private ActiveGraph ActivatedGraph 
    {
        get => activatedGraph;
        set => this.RaiseAndSetIfChanged(ref activatedGraph, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateRunsCommand { get; }

    public RunUpdateViewModel(IGraphRequestService<GraphVertexModel> graphService,
        IRangeRequestService<GraphVertexModel> rangeService,
        IStatisticsRequestService statisticsService,
        IAlgorithmsFactory algorithmsFactory,
        INeighborhoodLayerFactory neighborFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog log) : base(log)
    {
        this.messenger = messenger;
        this.graphService = graphService;
        this.rangeService = rangeService;
        this.statisticsService = statisticsService;
        this.neighborFactory = neighborFactory;
        this.algorithmsFactory = algorithmsFactory;
        UpdateRunsCommand = ReactiveCommand.CreateFromTask(ExecuteUpdate, CanUpdate()).DisposeWith(disposables);
        messenger.RegisterHandler<RunsSelectedMessage>(this, OnRunsSelected).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<GraphActivatedMessage>(this, OnGraphActivated).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphUpdatedMessage, int>(this,
            Tokens.AlgorithmUpdate, OnGraphUpdated).DisposeWith(disposables);
    }

    private void OnRunsSelected(RunsSelectedMessage msg)
    {
        Selected = msg.Value;
    }

    private void OnGraphActivated(GraphActivatedMessage msg)
    {
        ActivatedGraph = msg.Value.ActiveGraph;
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (!msg.Value.Contains(ActivatedGraph.Id))
        {
            return;
        }

        Selected = [];
        ActivatedGraph = ActiveGraph.Empty;
    }

    private IObservable<bool> CanUpdate()
    {
        return this.WhenAnyValue(x => x.Selected,
            x => x.ActivatedGraph, (s, g) => s.Length > 0 && g != ActiveGraph.Empty);
    }

    private async Task ExecuteUpdate()
    {
        await ExecuteSafe(async token =>
        {
            var models = await statisticsService.ReadStatisticsAsync(
                Selected.Select(x => x.Id), token).ConfigureAwait(false);
            var updated = await UpdateRunsAsync(models, ActivatedGraph.Graph, ActivatedGraph.Id).ConfigureAwait(false);
            messenger.Send(new RunsUpdatedMessage(updated));
        }).ConfigureAwait(false);
    }

    private async Task OnGraphUpdated(AwaitGraphUpdatedMessage msg)
    {
        var (graphToUpdate, graphId) = await EnsureGraphLoadedAsync(msg).ConfigureAwait(false);
        if (graphToUpdate == Graph<GraphVertexModel>.Empty)
        {
            return;
        }

        await NotifyRunsUpdatedAsync(graphId, graphToUpdate).ConfigureAwait(false);
    }

    private async Task<(Graph<GraphVertexModel> Graph, int GraphId)> EnsureGraphLoadedAsync(AwaitGraphUpdatedMessage msg)
    {
        var localGraph = ActivatedGraph.Graph;
        var graphId = ActivatedGraph.Id;
        var shouldReloadGraph = localGraph == Graph<GraphVertexModel>.Empty
            || msg.Value.Id != graphId;

        if (!shouldReloadGraph)
        {
            return (localGraph, graphId);
        }

        await ExecuteSafe(async token =>
        {
            var model = await graphService.ReadGraphAsync(msg.Value.Id, token).ConfigureAwait(false);
            localGraph = model.CreateGraph();
            graphId = model.Id;
            var layer = neighborFactory.CreateNeighborhoodLayer(model.Neighborhood);
            await layer.OverlayAsync(localGraph, token).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return (localGraph, graphId);
    }

    private async Task NotifyRunsUpdatedAsync(int graphId, Graph<GraphVertexModel> graphToUpdate)
    {
        await ExecuteSafe(async token =>
        {
            var models = await statisticsService.ReadStatisticsAsync(graphId, token).ConfigureAwait(false);
            var updated = await UpdateRunsAsync(models, graphToUpdate, graphId).ConfigureAwait(false);
            messenger.Send(new RunsUpdatedMessage(updated));
        }).ConfigureAwait(false);
    }

    private async Task<RunStatisticsModel[]> UpdateRunsAsync(
        IReadOnlyCollection<RunStatisticsModel> selectedStatistics,
        Graph<GraphVertexModel> graphToUpdate,
        int graphId)
    {
        var rangeModels = await rangeService.ReadRangeAsync(graphId).ConfigureAwait(false);
        var range = rangeModels.Select(x => graphToUpdate.Get(x.Position)).ToList();
        var updatedRuns = new List<RunStatisticsModel>();
        if (range.Count > 1)
        {
            using var cts = new CancellationTokenSource(GetTimeout());
            var ids = selectedStatistics.Select(x => x.Id).ToArray();
            var infos = await statisticsService.ReadStatisticsAsync(ids, cts.Token).ConfigureAwait(false);
            foreach (var info in infos)
            {
                var visitedCount = 0;
                void OnVertexProcessed(EventArgs e) => visitedCount++;
                var factory = algorithmsFactory.GetAlgorithmFactory(info.Algorithm);
                var algorithm = factory.CreateAlgorithm(range, info);
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
            await ExecuteSafe(async token =>
            {
                await statisticsService.UpdateStatisticsAsync(updatedRuns, token).ConfigureAwait(false);
            }, updatedRuns.Clear).ConfigureAwait(false);
        }
        return [.. updatedRuns];
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
