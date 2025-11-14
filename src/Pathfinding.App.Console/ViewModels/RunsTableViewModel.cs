using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunsTableViewModel : ViewModel, IRunsTableViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IStatisticsRequestService statisticsService;
    private readonly IGraphInfoRequestService graphInfoService;
    private readonly CompositeDisposable disposables = [];

    public ObservableCollection<RunInfoModel> Runs { get; } = [];

    public ReactiveCommand<int[], Unit> SelectRunsCommand { get; }

    private ActiveGraph ActivatedGraph { get; set; } = ActiveGraph.Empty;

    public RunsTableViewModel(IStatisticsRequestService statisticsService,
        IGraphInfoRequestService graphInfoService,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.statisticsService = statisticsService;
        this.graphInfoService = graphInfoService;

        messenger.RegisterAsyncHandler<RunsCreatedMessaged>(this, OnRunCreated).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphActivatedMessage, int>(this, Tokens.RunsTable, OnGraphActivatedMessage).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<RunsUpdatedMessage>(this, OnRunsUpdated).DisposeWith(disposables);
        messenger.RegisterAsyncHandler<RunsDeletedMessage>(this, OnRunsDeleteMessage).DisposeWith(disposables);

        SelectRunsCommand = ReactiveCommand.Create<int[]>(SelectRuns);
    }

    private void SelectRuns(int[] selected)
    {
        var selectedRuns = Runs.Where(x => selected.Contains(x.Id)).ToArray();
        messenger.Send(new RunsSelectedMessage(selectedRuns));
    }

    private async Task OnGraphActivatedMessage(AwaitGraphActivatedMessage msg)
    {
        await ExecuteSafe(async token =>
        {
            ActivatedGraph = msg.Value.ActiveGraph;
            var statistics = await statisticsService
                .ReadStatisticsAsync(ActivatedGraph.Id, token)
                .ConfigureAwait(false);
            
            Runs.Clear();
            Runs.Add(statistics.ToRunInfo());
        }).ConfigureAwait(false);
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraph.Id))
        {
            Runs.Clear();
            ActivatedGraph = ActiveGraph.Empty;
        }
    }

    private void OnRunsUpdated(RunsUpdatedMessage msg)
    {
        var runs = Runs.ToDictionary(x => x.Id);
        foreach (var model in msg.Value)
        {
            if (runs.TryGetValue(model.Id, out var run))
            {
                run.ResultStatus = model.ResultStatus;
                run.Visited = model.Visited;
                run.Steps = model.Steps;
                run.Cost = model.Cost;
                run.Elapsed = model.Elapsed;
            }
        }
    }

    private async Task OnRunsDeleteMessage(RunsDeletedMessage msg)
    {
        await ExecuteSafe(async token =>
        {
            var toDelete = Runs
                .Where(x => msg.Value.Contains(x.Id))
                .ToArray();
            Runs.Remove(toDelete);
            if (Runs.Count == 0)
            {
                messenger.Send(new GraphStateChangedMessage((ActivatedGraph.Id, GraphStatuses.Editable)));
                var graphInfo = await graphInfoService.ReadGraphInfoAsync(ActivatedGraph.Id, token).ConfigureAwait(false);
                graphInfo.Status = GraphStatuses.Editable;
                await graphInfoService.UpdateGraphInfoAsync(graphInfo, token).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    private async Task OnRunCreated(RunsCreatedMessaged msg)
    {
        await ExecuteSafe(async token =>
        {
            int previousCount = Runs.Count;
            Runs.Add(msg.Value.ToRunInfo());
            if (previousCount == 0)
            {
                messenger.Send(new GraphStateChangedMessage((ActivatedGraph.Id, GraphStatuses.Readonly)));
                var graphInfo = await graphInfoService.ReadGraphInfoAsync(ActivatedGraph.Id, token).ConfigureAwait(false);
                graphInfo.Status = GraphStatuses.Readonly;
                await graphInfoService.UpdateGraphInfoAsync(graphInfo, token).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
