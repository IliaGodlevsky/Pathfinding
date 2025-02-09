using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Model;
using Pathfinding.App.Console.ViewModel.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModel
{
    internal sealed class GraphDeleteViewModel : BaseViewModel, IGraphDeleteViewModel
    {
        private readonly IMessenger messenger;
        private readonly IRequestService<GraphVertexModel> service;
        private readonly ILog logger;

        private int[] selectedGraphIds = [];
        public int[] SelectedGraphIds
        {
            get => selectedGraphIds;
            set => this.RaiseAndSetIfChanged(ref selectedGraphIds, value);
        }

        public ReactiveCommand<Unit, Unit> DeleteGraphCommand { get; }

        public GraphDeleteViewModel(
            [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
            IRequestService<GraphVertexModel> service,
            ILog logger)
        {
            DeleteGraphCommand = ReactiveCommand.CreateFromTask(DeleteGraph, CanDelete());
            this.messenger = messenger;
            this.service = service;
            this.logger = logger;
            messenger.Register<GraphSelectedMessage>(this, OnGraphSelected);
        }

        private IObservable<bool> CanDelete()
        {
            return this.WhenAnyValue(x => x.SelectedGraphIds,
                ids => ids.Length > 0);
        }

        private async Task DeleteGraph()
        {
            await ExecuteSafe(async () =>
            {
                var isDeleted = await service.DeleteGraphsAsync(selectedGraphIds)
                    .ConfigureAwait(false);
                if (isDeleted)
                {
                    var graphs = SelectedGraphIds.ToArray();
                    SelectedGraphIds = [];
                    messenger.Send(new GraphsDeletedMessage(graphs));
                }
            }, logger.Error).ConfigureAwait(false);
        }

        private void OnGraphSelected(object recipient, GraphSelectedMessage msg)
        {
            SelectedGraphIds = msg.Graphs.Select(x => x.Id).ToArray();
        }
    }
}
