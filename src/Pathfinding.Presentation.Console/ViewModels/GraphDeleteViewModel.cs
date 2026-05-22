using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Pathfinding.Presentation.Console.ViewModels;

internal sealed class GraphDeleteViewModel : ViewModel, IGraphDeleteViewModel, IDisposable
{
    private readonly IMessenger messenger;
    private readonly IGraphInfoRequestService service;
    private readonly CompositeDisposable disposables = [];

    private int[] selectedGraphIds = [];
    public int[] SelectedGraphIds
    {
        get => selectedGraphIds;
        set => this.RaiseAndSetIfChanged(ref selectedGraphIds, value);
    }

    public ReactiveCommand<Unit, Unit> DeleteGraphCommand { get; }

    public GraphDeleteViewModel(
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        IGraphInfoRequestService service,
        ILog logger) : base(logger)
    {
        this.service = service;
        this.messenger = messenger;
        DeleteGraphCommand = ReactiveCommand.CreateFromTask(DeleteGraph, CanDelete()).DisposeWith(disposables);
        messenger.RegisterHandler<GraphsSelectedMessage>(this, OnGraphSelected).DisposeWith(disposables);
    }

    private IObservable<bool> CanDelete()
    {
        return this.WhenAnyValue(x => x.SelectedGraphIds,
            ids => ids.Length > 0);
    }

    private async Task DeleteGraph()
    {
        await ExecuteSafe(async token =>
        {
            var isDeleted = await service
                .DeleteGraphsAsync(SelectedGraphIds, token)
                .ConfigureAwait(false);
            if (isDeleted)
            {
                var graphs = SelectedGraphIds.ToArray();
                SelectedGraphIds = [];
                messenger.Send(new GraphsDeletedMessage(graphs));
            }
        }).ConfigureAwait(false);
    }

    private void OnGraphSelected(GraphsSelectedMessage msg)
    {
        SelectedGraphIds = [.. msg.Value.Select(x => x.Id)];
    }

    public void Dispose()
    {
        disposables.Dispose();
    }
}
