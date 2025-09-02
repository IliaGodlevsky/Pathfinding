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

internal sealed class RunsTableViewModel : BaseViewModel, IRunsTableViewModel, IDisposable 
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly CompositeDisposable disposables = [];

    public ObservableCollection<RunInfoModel> Runs { get; } = [];

    public ReactiveCommand<int[], Unit> SelectRunsCommand { get; }

    private int ActivatedGraphId { get; set; }

    public RunsTableViewModel(IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger) :base(logger)
    {
        this.messenger = messenger;
        this.service = service;

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
            var statistics = await service
                .ReadStatisticsAsync(msg.Value.GraphId, token)
                .ConfigureAwait(false);
            ActivatedGraphId = msg.Value.GraphId;
            Runs.Clear();
            Runs.Add(statistics.ToRunInfo());
        }).ConfigureAwait(false);
    }

    private void OnGraphDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraphId))
        {
            Runs.Clear();
            ActivatedGraphId = 0;
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
            if (toDelete.Length == Runs.Count)
            {
                Runs.Clear();
                messenger.Send(new GraphStateChangedMessage((ActivatedGraphId, GraphStatuses.Editable)));
                var graphInfo = await service.ReadGraphInfoAsync(ActivatedGraphId, token).ConfigureAwait(false);
                graphInfo.Status = GraphStatuses.Editable;
                await service.UpdateGraphInfoAsync(graphInfo, token).ConfigureAwait(false);
            }
            else
            {
                Runs.Remove(toDelete);
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
                messenger.Send(new GraphStateChangedMessage((ActivatedGraphId, GraphStatuses.Readonly)));
                var graphInfo = await service.ReadGraphInfoAsync(ActivatedGraphId, token).ConfigureAwait(false);
                graphInfo.Status = GraphStatuses.Readonly;
                await service.UpdateGraphInfoAsync(graphInfo, token).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
