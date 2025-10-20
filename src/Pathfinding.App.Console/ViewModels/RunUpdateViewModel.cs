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
    private readonly IRequestService<GraphVertexModel> service;
    private readonly IAlgorithmsFactory algorithmsFactory;
    private readonly INeighborhoodLayerFactory neighborFactory;
    private readonly CompositeDisposable disposables = [];

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
        IAlgorithmsFactory algorithmsFactory,
        INeighborhoodLayerFactory neighborFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog log) : base(log)
    {
        this.messenger = messenger;
        this.service = service;
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
        Graph = msg.Value.Graph;
        ActivatedGraphId = msg.Value.GraphId;
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
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
        await ExecuteSafe(async token =>
        {
            var models = await service.ReadStatisticsAsync(
                Selected.Select(x => x.Id), token).ConfigureAwait(false);
            var updated = await UpdateRunsAsync(models, Graph, ActivatedGraphId).ConfigureAwait(false);
            messenger.Send(new RunsUpdatedMessage(updated));
        }).ConfigureAwait(false);
    }

    private async Task OnGraphUpdated(AwaitGraphUpdatedMessage msg)
    {
        var id = ActivatedGraphId;
        var local = Graph;
        if ((local != Graph<GraphVertexModel>.Empty 
            && msg.Value.Id != ActivatedGraphId) || local == Graph<GraphVertexModel>.Empty)
        {
            await ExecuteSafe(async token =>
            {
                var model = await service.ReadGraphAsync(msg.Value.Id, token).ConfigureAwait(false);
                local = model.CreateGraph();
                id = model.Id;
                var layer = neighborFactory.CreateNeighborhoodLayer(model.Neighborhood);
                await layer.OverlayAsync(local, token).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
        if (local != Graph<GraphVertexModel>.Empty)
        {
            await ExecuteSafe(async token =>
            {
                var models = await service.ReadStatisticsAsync(id, token).ConfigureAwait(false);
                var updated = await UpdateRunsAsync(models, local, id).ConfigureAwait(false);
                messenger.Send(new RunsUpdatedMessage(updated));
            }).ConfigureAwait(false);
        }
    }

    private async Task<RunStatisticsModel[]> UpdateRunsAsync(
        IReadOnlyCollection<RunStatisticsModel> selectedStatistics,
        Graph<GraphVertexModel> graphToUpdate,
        int graphId)
    {
        var rangeModels = await service.ReadRangeAsync(graphId).ConfigureAwait(false);
        var range = rangeModels.Select(x => graphToUpdate.Get(x.Position)).ToList();
        var updatedRuns = new List<RunStatisticsModel>();
        if (range.Count > 1)
        {
            using var cts = new CancellationTokenSource(GetTimeout());
            var ids = selectedStatistics.Select(x => x.Id).ToArray();
            var infos = await service.ReadStatisticsAsync(ids, cts.Token).ConfigureAwait(false);
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
                await service.UpdateStatisticsAsync(updatedRuns, token).ConfigureAwait(false);
            }, updatedRuns.Clear).ConfigureAwait(false);
        }
        return [.. updatedRuns];
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
