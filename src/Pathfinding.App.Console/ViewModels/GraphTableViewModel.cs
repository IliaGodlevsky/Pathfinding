using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.ViewModels
{
    internal sealed class GraphTableViewModel : BaseViewModel, IGraphTableViewModel
    {
        private readonly IRequestService<GraphVertexModel> service;
        private readonly IMessenger messenger;
        private readonly ILog logger;

        public ReactiveCommand<Unit, Unit> LoadGraphsCommand { get; }

        public ReactiveCommand<int, Unit> ActivateGraphCommand { get; }

        public ReactiveCommand<int[], Unit> SelectGraphsCommand { get; }

        public ObservableCollection<GraphInfoModel> Graphs { get; } = [];

        private int ActivatedGraphId { get; set; } = 0;

        public GraphTableViewModel(
            IRequestService<GraphVertexModel> service,
            [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
            ILog logger)
        {
            this.service = service;
            this.messenger = messenger;
            this.logger = logger;
            messenger.RegisterAsyncHandler<AsyncGraphUpdatedMessage, int>(this, Tokens.GraphTable, OnGraphUpdated);
            messenger.Register<GraphCreatedMessage>(this, OnGraphCreated);
            messenger.Register<GraphsDeletedMessage>(this, OnGraphDeleted);
            messenger.Register<ObstaclesCountChangedMessage>(this, OnObstaclesCountChanged);
            messenger.Register<GraphStateChangedMessage>(this, GraphStateChanged);
            LoadGraphsCommand = ReactiveCommand.CreateFromTask(LoadGraphs);
            ActivateGraphCommand = ReactiveCommand.CreateFromTask<int>(ActivatedGraph);
            SelectGraphsCommand = ReactiveCommand.Create<int[]>(SelectGraphs);
        }

        private void SelectGraphs(int[] selected)
        {
            var graphs = Graphs.Where(x => selected.Contains(x.Id)).ToArray();
            messenger.Send(new GraphSelectedMessage(graphs));
        }

        private async Task ActivatedGraph(int model)
        {
            await ExecuteSafe(async () =>
            {
                var graphModel = await service.ReadGraphAsync(model).ConfigureAwait(false);
                var graph = new Graph<GraphVertexModel>(graphModel.Vertices, graphModel.DimensionSizes);
                var layers = graphModel.ToLayers();
                await layers.OverlayAsync(graph).ConfigureAwait(false);
                ActivatedGraphId = graphModel.Id;
                messenger.Send(new GraphActivatedMessage(graphModel), Tokens.GraphField);
                await messenger.SendAsync(new AsyncGraphActivatedMessage(graphModel), Tokens.PathfindingRange);
                await messenger.SendAsync(new AsyncGraphActivatedMessage(graphModel), Tokens.RunsTable);
                messenger.Send(new GraphActivatedMessage(graphModel));
            }, logger.Error).ConfigureAwait(false);
        }

        private async Task LoadGraphs()
        {
            await ExecuteSafe(async () =>
            {
                Graphs.Clear();
                var infos = (await service.ReadAllGraphInfoAsync().ConfigureAwait(false))
                    .ToGraphInfo();
                Graphs.Add(infos);
            }, logger.Error).ConfigureAwait(false);
        }

        private void OnObstaclesCountChanged(object recipient, ObstaclesCountChangedMessage msg)
        {
            var graph = Graphs.FirstOrDefault(x => x.Id == msg.GraphId);
            if (graph != null)
            {
                graph.ObstaclesCount += msg.Delta;
            }
        }

        private void GraphStateChanged(object recipient, GraphStateChangedMessage msg)
        {
            var graph = Graphs.FirstOrDefault(x => x.Id == msg.Id);
            if (graph != null)
            {
                graph.Status = msg.Status;
            }
        }

        private async Task OnGraphUpdated(object recipient, AsyncGraphUpdatedMessage msg)
        {
            var model = Graphs.FirstOrDefault(x => x.Id == msg.Model.Id);
            if (model != null)
            {
                model.Name = msg.Model.Name;
                model.Neighborhood = msg.Model.Neighborhood;
                model.SmoothLevel = msg.Model.SmoothLevel;
                if (ActivatedGraphId == model.Id)
                {
                    await ActivatedGraph(ActivatedGraphId);
                }
            }

            msg.Signal(Unit.Default);
        }

        private void OnGraphCreated(object recipient, GraphCreatedMessage msg)
        {
            Graphs.Add(msg.Models);
        }

        private void OnGraphDeleted(object recipient, GraphsDeletedMessage msg)
        {
            var graphs = Graphs
                .Where(x => msg.GraphIds.Contains(x.Id))
                .ToList();
            Graphs.Remove(graphs);
        }
    }
}
