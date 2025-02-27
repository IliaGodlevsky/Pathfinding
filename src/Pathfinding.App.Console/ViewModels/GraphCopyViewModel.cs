using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels
{
    internal sealed class GraphCopyViewModel : BaseViewModel, IGraphCopyViewModel
    {
        private readonly IMessenger messenger;
        private readonly IRequestService<GraphVertexModel> service;
        private readonly ILog log;

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
            ILog log)
        {
            this.messenger = messenger;
            this.service = service;
            this.log = log;
            messenger.Register<GraphSelectedMessage>(this, OnGraphSelected);
            messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
            CopyGraphCommand = ReactiveCommand.CreateFromTask(ExecuteCopy, CanExecute());
        }

        private void OnGraphSelected(object recipient, GraphSelectedMessage msg)
        {
            SelectedGraphIds = msg.Graphs.Select(x => x.Id).ToArray();
        }

        private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
        {
            SelectedGraphIds = SelectedGraphIds.Where(x => !msg.GraphIds.Contains(x)).ToArray();
        }

        private async Task ExecuteCopy()
        {
            await ExecuteSafe(async () =>
            {
                var copies = await service.ReadSerializationHistoriesAsync(SelectedGraphIds)
                    .ConfigureAwait(false);
                var histories = await service.CreatePathfindingHistoriesAsync(copies.Histories)
                    .ConfigureAwait(false);
                var graphs = histories.Select(x => x.Graph).ToGraphInfo();
                messenger.Send(new GraphCreatedMessage(graphs));
            }, log.Error).ConfigureAwait(false);
        }

        private IObservable<bool> CanExecute()
        {
            return this.WhenAnyValue(x => x.SelectedGraphIds,
                ids => ids.Length > 0);
        }
    }
}
