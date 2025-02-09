using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Model;
using Pathfinding.App.Console.ViewModel.Interface;
using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModel
{
    internal sealed class GraphAssembleViewModel : BaseViewModel,
        IGraphAssembleViewModel,
        IRequireGraphNameViewModel,
        IRequireGraphParametresViewModel,
        IRequireSmoothLevelViewModel,
        IRequireNeighborhoodNameViewModel
    {
        private static readonly InclusiveValueRange<int> CostRange = (9, 1);

        private readonly IRequestService<GraphVertexModel> service;
        private readonly IGraphAssemble<GraphVertexModel> graphAssemble;
        private readonly IMessenger messenger;
        private readonly ILog logger;

        private string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private int width;
        public int Width
        {
            get => width;
            set => this.RaiseAndSetIfChanged(ref width, value);
        }

        private int length;
        public int Length
        {
            get => length;
            set => this.RaiseAndSetIfChanged(ref length, value);
        }

        private int obstacles;
        public int Obstacles
        {
            get => obstacles;
            set => this.RaiseAndSetIfChanged(ref obstacles, value);
        }

        private SmoothLevels level;
        public SmoothLevels SmoothLevel
        {
            get => level;
            set => this.RaiseAndSetIfChanged(ref level, value);
        }

        private Neighborhoods neighborhood;
        public Neighborhoods Neighborhood
        {
            get => neighborhood;
            set => this.RaiseAndSetIfChanged(ref neighborhood, value);
        }

        public ReactiveCommand<Unit, Unit> AssembleGraphCommand { get; }

        public GraphAssembleViewModel(IRequestService<GraphVertexModel> service,
            IGraphAssemble<GraphVertexModel> graphAssemble,
            [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
            ILog logger)
        {
            this.service = service;
            this.messenger = messenger;
            this.logger = logger;
            this.graphAssemble = graphAssemble;
            AssembleGraphCommand = ReactiveCommand.CreateFromTask(CreateGraph, CanExecute());
        }

        private IObservable<bool> CanExecute()
        {
            return this.WhenAnyValue(
                x => x.Width,
                x => x.Length,
                x => x.Obstacles,
                x => x.Name,
                (width, length, obstacles, name) =>
                {
                    return width > 0 && length > 0
                        && obstacles >= 0
                        && !string.IsNullOrEmpty(name);
                });
        }

        private async Task CreateGraph()
        {
            await ExecuteSafe(async () =>
            {
                var random = Random.Shared;
                var costLayer = new VertexCostLayer(CostRange, range => new VertexCost(random.Next(
                    range.LowerValueOfRange, range.UpperValueOfRange + 1), range));
                var obstacleLayer = new ObstacleLayer(Obstacles);
                var layers = new Layers(costLayer, obstacleLayer);
                var graph = await graphAssemble.AssembleGraphAsync(layers, Width, Length)
                    .ConfigureAwait(false);
                var request = new CreateGraphRequest<GraphVertexModel>()
                {
                    Graph = graph,
                    Name = Name,
                    Neighborhood = Neighborhood,
                    SmoothLevel = SmoothLevel,
                    Status = GraphStatuses.Editable
                };
                var graphModel = await service.CreateGraphAsync(request).ConfigureAwait(false);
                var info = graphModel.ToGraphInformationModel().ToGraphInfo();
                messenger.Send(new GraphCreatedMessage([info]));
            }, logger.Error).ConfigureAwait(false);
        }
    }
}
