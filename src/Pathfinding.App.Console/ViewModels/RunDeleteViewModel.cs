using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunDeleteViewModel : BaseViewModel, IRunDeleteViewModel
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly ILog logger;

    private int[] selectedRunsIds = [];
    public int[] SelectedRunsIds
    {
        get => selectedRunsIds;
        set => this.RaiseAndSetIfChanged(ref selectedRunsIds, value);
    }

    private int ActivatedGraph { get; set; }

    public ReactiveCommand<Unit, Unit> DeleteRunsCommand { get; }

    public RunDeleteViewModel(
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        IRequestService<GraphVertexModel> service,
        ILog logger)
    {
        this.messenger = messenger;
        this.service = service;
        this.logger = logger;
        messenger.Register<RunsSelectedMessage>(this, OnRunsSelected);
        messenger.Register<GraphsDeletedMessage>(this, OnGraphsDeleted);
        messenger.Register<GraphActivatedMessage>(this, OnGraphActivated);
        DeleteRunsCommand = ReactiveCommand.CreateFromTask(DeleteRuns, CanDelete());
    }

    private IObservable<bool> CanDelete()
    {
        return this.WhenAnyValue(x => x.SelectedRunsIds,
            ids => ids.Length > 0);
    }

    private async Task DeleteRuns()
    {
        await ExecuteSafe(async () =>
        {
            var isDeleted = await service.DeleteRunsAsync(SelectedRunsIds).ConfigureAwait(false);
            if (isDeleted)
            {
                var runs = SelectedRunsIds.ToArray();
                SelectedRunsIds = [];
                messenger.Send(new RunsDeletedMessage(runs));
            }
        }, logger.Error).ConfigureAwait(false);
    }

    private void OnRunsSelected(object recipient, RunsSelectedMessage msg)
    {
        SelectedRunsIds = [.. msg.Value.Select(x => x.Id)];
    }

    private void OnGraphActivated(object recipient, GraphActivatedMessage msg)
    {
        ActivatedGraph = msg.Value.GraphId;
    }

    private void OnGraphsDeleted(object recipient, GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraph))
        {
            ActivatedGraph = 0;
            SelectedRunsIds = [];
        }
    }
}
