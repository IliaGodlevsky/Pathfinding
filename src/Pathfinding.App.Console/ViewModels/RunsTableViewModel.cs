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

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunsTableViewModel : BaseViewModel, IRunsTableViewModel
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly ILog logger;

    public ObservableCollection<RunInfoModel> Runs { get; } = [];

    public ReactiveCommand<int[], Unit> SelectRunsCommand { get; }

    private int ActivatedGraphId { get; set; }

    public RunsTableViewModel(IRequestService<GraphVertexModel> service,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.logger = logger;

        messenger.RegisterAsyncHandler<RunsCreatedMessaged>(this, OnRunCreated);
        messenger.RegisterAsyncHandler<AsyncGraphActivatedMessage, int>(this, Tokens.RunsTable, OnGraphActivatedMessage);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
        messenger.Register<RunsUpdatedMessage>(this, OnRunsUpdated);
        messenger.RegisterAsyncHandler<RunsDeletedMessage>(this, OnRunsDeleteMessage);

        SelectRunsCommand = ReactiveCommand.Create<int[]>(SelectRuns);
    }

    private void SelectRuns(int[] selected)
    {
        var selectedRuns = Runs.Where(x => selected.Contains(x.Id)).ToArray();
        messenger.Send(new RunsSelectedMessage(selectedRuns));
    }

    private async Task OnGraphActivatedMessage(object recipient, AsyncGraphActivatedMessage msg)
    {
        await ExecuteSafe(async () =>
        {
            var statistics = await service.ReadStatisticsAsync(msg.Value.GraphId)
                .ConfigureAwait(false);
            var models = statistics.ToRunInfo();
            ActivatedGraphId = msg.Value.GraphId;
            Runs.Clear();
            Runs.Add(models);
            msg.SetCompleted(true);
        },(ex, message) =>
        {
            msg.SetCompleted(false);
            logger.Error(ex, message);
        }).ConfigureAwait(false);
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraphId))
        {
            Runs.Clear();
            ActivatedGraphId = 0;
        }
    }

    private void OnRunsUpdated(object recipient, RunsUpdatedMessage msg)
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

    private async Task OnRunsDeleteMessage(object recipient, RunsDeletedMessage msg)
    {
        var toDelete = Runs.Where(x => msg.Value.Contains(x.Id)).ToArray();
        if (toDelete.Length == Runs.Count)
        {
            Runs.Clear();
            messenger.Send(new GraphStateChangedMessage((ActivatedGraphId, GraphStatuses.Editable)));
            var graphInfo = await service.ReadGraphInfoAsync(ActivatedGraphId).ConfigureAwait(false);
            graphInfo.Status = GraphStatuses.Editable;
            await service.UpdateGraphInfoAsync(graphInfo).ConfigureAwait(false);
        }
        else
        {
            Runs.Remove(toDelete);
        }
    }

    private async Task OnRunCreated(object recipient, RunsCreatedMessaged msg)
    {
        int previousCount = Runs.Count;
        Runs.Add(msg.Value.ToRunInfo());
        if (previousCount == 0)
        {
            messenger.Send(new GraphStateChangedMessage((ActivatedGraphId, GraphStatuses.Readonly)));
            var graphInfo = await service.ReadGraphInfoAsync(ActivatedGraphId).ConfigureAwait(false);
            graphInfo.Status = GraphStatuses.Readonly;
            await service.UpdateGraphInfoAsync(graphInfo).ConfigureAwait(false);
        }
    }
}
