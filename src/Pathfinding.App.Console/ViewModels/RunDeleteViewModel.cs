using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunDeleteViewModel : ViewModel, IRunDeleteViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IStatisticsRequestService statisticsService;
    private readonly CompositeDisposable disposables = [];

    private int[] selectedRunsIds = [];
    public int[] SelectedRunsIds
    {
        get => selectedRunsIds;
        set => this.RaiseAndSetIfChanged(ref selectedRunsIds, value);
    }

    private ActiveGraph ActivatedGraph { get; set; }

    public ReactiveCommand<Unit, Unit> DeleteRunsCommand { get; }

    public RunDeleteViewModel(
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        IStatisticsRequestService statisticsService,
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.statisticsService = statisticsService;
        messenger.RegisterHandler<RunsSelectedMessage>(this, OnRunsSelected).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphsDeleted).DisposeWith(disposables);
        messenger.RegisterAwaitHandler<AwaitGraphActivatedMessage>(this, OnGraphActivated).DisposeWith(disposables);
        DeleteRunsCommand = ReactiveCommand.CreateFromTask(DeleteRuns, CanDelete());
    }

    private IObservable<bool> CanDelete()
    {
        return this.WhenAnyValue(x => x.SelectedRunsIds,
            ids => ids.Length > 0);
    }

    private async Task DeleteRuns()
    {
        await ExecuteSafe(async token =>
        {
            bool isDeleted = await statisticsService
                .DeleteRunsAsync(SelectedRunsIds, token)
                .ConfigureAwait(false);
            if (isDeleted)
            {
                var runs = SelectedRunsIds.ToArray();
                SelectedRunsIds = [];
                messenger.Send(new RunsDeletedMessage(runs));
            }
        }).ConfigureAwait(false);
    }

    private void OnRunsSelected(RunsSelectedMessage msg)
    {
        SelectedRunsIds = [.. msg.Value.Select(x => x.Id)];
    }

    private Task OnGraphActivated(AwaitGraphActivatedMessage msg)
    {
        ActivatedGraph = msg.Value.ActiveGraph;
        SelectedRunsIds = [];
        return Task.CompletedTask;
    }

    private void OnGraphsDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraph.Id))
        {
            ActivatedGraph = ActiveGraph.Empty;
            SelectedRunsIds = [];
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
