using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class RunDeleteViewModel : ViewModel, IRunDeleteViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly CompositeDisposable disposables = [];

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
        ILog logger) : base(logger)
    {
        this.messenger = messenger;
        this.service = service;
        messenger.RegisterHandler<RunsSelectedMessage>(this, OnRunsSelected).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphsDeleted).DisposeWith(disposables);
        messenger.RegisterHandler<GraphActivatedMessage>(this, OnGraphActivated).DisposeWith(disposables);
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
            var isDeleted = await service.DeleteRunsAsync(
                SelectedRunsIds, token).ConfigureAwait(false);
            if (isDeleted)
            {
                var runs = SelectedRunsIds.ToArray();
                SelectedRunsIds = [];
                messenger.Send(new RunsDeletedMessage(runs));
            }
        }).ConfigureAwait(false);
    }

    protected override TimeSpan GetTimeout()
    {
        return base.GetTimeout() * SelectedRunsIds.Length;
    }

    private void OnRunsSelected(RunsSelectedMessage msg)
    {
        SelectedRunsIds = [.. msg.Value.Select(x => x.Id)];
    }

    private void OnGraphActivated(GraphActivatedMessage msg)
    {
        ActivatedGraph = msg.Value.GraphId;
    }

    private void OnGraphsDeleted(GraphsDeletedMessage msg)
    {
        if (msg.Value.Contains(ActivatedGraph))
        {
            ActivatedGraph = 0;
            SelectedRunsIds = [];
        }
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
