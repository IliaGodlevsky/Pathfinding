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

internal sealed class GraphCopyViewModel : BaseViewModel, IGraphCopyViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IRequestService<GraphVertexModel> service;
    private readonly CompositeDisposable disposables = [];

    public ReactiveCommand<Unit, Unit> CopyGraphCommand { get; }

    private int[] selectedGraphIds = [];
    public int[] SelectedGraphIds
    {
        get => selectedGraphIds;
        set => this.RaiseAndSetIfChanged(ref selectedGraphIds, value);
    }

    public GraphCopyViewModel(
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        IRequestService<GraphVertexModel> service,
        ILog log) : base(log)
    {
        this.messenger = messenger;
        this.service = service;
        messenger.RegisterHandler<GraphsSelectedMessage>(this, OnGraphSelected).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsDeletedMessage>(this, OnGraphDeleted).DisposeWith(disposables);
        CopyGraphCommand = ReactiveCommand.CreateFromTask(ExecuteCopy, CanExecute()).DisposeWith(disposables);
    }

    private void OnGraphSelected(object recipient, GraphsSelectedMessage msg)
    {
        SelectedGraphIds = [.. msg.Value.Select(x => x.Id)];
    }

    private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
    {
        SelectedGraphIds = [.. SelectedGraphIds.Where(x => !msg.Value.Contains(x))];
    }

    private async Task ExecuteCopy()
    {
        await ExecuteSafe(async () =>
        {
            var timeout = Timeout * SelectedGraphIds.Length;
            using var cts = new CancellationTokenSource(timeout);
            var copies = await service.ReadSerializationHistoriesAsync(
                SelectedGraphIds, cts.Token).ConfigureAwait(false);
            var histories = await service.CreatePathfindingHistoriesAsync(
                copies.Histories, cts.Token).ConfigureAwait(false);
            var graphs = histories.Select(x => x.Graph).ToGraphInfo();
            messenger.Send(new GraphsCreatedMessage(graphs));
        }).ConfigureAwait(false);
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(x => x.SelectedGraphIds,
            ids => ids.Length > 0);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
