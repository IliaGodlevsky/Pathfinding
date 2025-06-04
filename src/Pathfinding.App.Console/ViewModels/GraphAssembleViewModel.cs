using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels;

internal sealed class GraphAssembleViewModel : BaseViewModel,
    IGraphAssembleViewModel,
    IRequireGraphNameViewModel,
    IRequireGraphParametresViewModel,
    IRequireSmoothLevelViewModel,
    IRequireNeighborhoodNameViewModel
{
    private static readonly InclusiveValueRange<int> WidthRange = (52, 1);
    private static readonly InclusiveValueRange<int> LengthRange = (51, 1);
    private static readonly InclusiveValueRange<int> ObstaclesRange = (99, 0);
    private static readonly InclusiveValueRange<int> CostRange = (9, 1);

    private readonly INeighborhoodLayerFactory neighborFactory;
    private readonly ISmoothLevelFactory smoothLevelFactory;
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
        set { width = WidthRange.ReturnInRange(value); this.RaisePropertyChanged(); }
    }

    private int length;
    public int Length
    {
        get => length;
        set { length = LengthRange.ReturnInRange(value); this.RaisePropertyChanged(); }
    }

    private int obstacles;
    public int Obstacles
    {
        get => obstacles;
        set { obstacles = ObstaclesRange.ReturnInRange(value); this.RaisePropertyChanged(); }
    }

    private SmoothLevels level;
    public SmoothLevels SmoothLevel
    {
        get => level;
        set => this.RaiseAndSetIfChanged(ref level, value);
    }

    public IReadOnlyCollection<SmoothLevels> AllowedLevels
        => smoothLevelFactory.Allowed;

    private Neighborhoods neighborhood;
    public Neighborhoods Neighborhood
    {
        get => neighborhood;
        set => this.RaiseAndSetIfChanged(ref neighborhood, value);
    }

    public IReadOnlyCollection<Neighborhoods> AllowedNeighborhoods
        => neighborFactory.Allowed;

    public ReactiveCommand<Unit, Unit> AssembleGraphCommand { get; }

    public GraphAssembleViewModel(
        IRequestService<GraphVertexModel> service,
        IGraphAssemble<GraphVertexModel> graphAssemble,
        ISmoothLevelFactory smoothLevelFactory,
        INeighborhoodLayerFactory neighborFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger)
    {
        this.service = service;
        this.messenger = messenger;
        this.logger = logger;
        this.graphAssemble = graphAssemble;
        this.neighborFactory = neighborFactory;
        this.smoothLevelFactory = smoothLevelFactory;
        AssembleGraphCommand = ReactiveCommand.CreateFromTask(CreateGraph, CanExecute());
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(
            x => x.Width,
            x => x.Length,
            x => x.Obstacles,
            x => x.Name,
            (x, y, z, a) => x > 0 && y > 0
                && z >= 0 && !string.IsNullOrEmpty(a)
        );
    }

    private async Task CreateGraph()
    {
        await ExecuteSafe(async () =>
        {
            var layers = GetLayers();
            var graph = await graphAssemble.AssembleGraphAsync(layers, Width, Length)
                .ConfigureAwait(false);
            var request = new CreateGraphRequest<GraphVertexModel>
            {
                Graph = graph,
                Name = Name,
                Neighborhood = Neighborhood,
                SmoothLevel = SmoothLevel,
                Status = GraphStatuses.Editable
            };
            var graphModel = await service.CreateGraphAsync(request).ConfigureAwait(false);
            var info = graphModel.ToGraphInformationModel().ToGraphInfo();
            messenger.Send(new GraphsCreatedMessage([info]));
        }, logger.Error).ConfigureAwait(false);
    }

    private Layers GetLayers()
    {
        var random = Random.Shared;
        var costLayer = new VertexCostLayer(CostRange, range => new VertexCost(random.Next(
            range.LowerValueOfRange, range.UpperValueOfRange + 1), range));
        var obstacleLayer = new ObstacleLayer(Obstacles);
        var smoothLayer = smoothLevelFactory.CreateLayer(SmoothLevel);
        var neighborhoodLayer = neighborFactory.CreateNeighborhoodLayer(Neighborhood);
        return new (neighborhoodLayer, costLayer, obstacleLayer, smoothLayer);
    }
}
