using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Data;
using Pathfinding.Domain.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Layers;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels;

internal sealed class GraphAssembleViewModel : ViewModel,
    IGraphAssembleViewModel,
    IRequireGraphNameViewModel,
    IRequireGraphParametresViewModel,
    IRequireSmoothLevelViewModel,
    IRequireNeighborhoodNameViewModel,
    IDisposable
{
    private static readonly InclusiveValueRange<int> WidthRange = (Settings.Default.MaxGraphWidth, 1);
    private static readonly InclusiveValueRange<int> LengthRange = (Settings.Default.MaxGraphLength, 1);
    private static readonly InclusiveValueRange<int> DepthRange = (0, 5);
    private static readonly InclusiveValueRange<int> ObstaclesRange = (99, 0);
    private static readonly InclusiveValueRange<int> CostRange = (99, 1);

    private readonly INeighborhoodLayerFactory neighborFactory;
    private readonly ISmoothLevelFactory smoothLevelFactory;
    private readonly IGraphRequestService<GraphVertexModel> service;
    private readonly IGraphAssemble<GraphVertexModel> graphAssemble;
    private readonly IMessenger messenger;

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

    private int depth;
    public int Depth
    {
        get => depth;
        set { depth = DepthRange.ReturnInRange(value); this.RaisePropertyChanged(); }
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

    private Neighborhoods neighborhood;
    public Neighborhoods Neighborhood
    {
        get => neighborhood;
        set => this.RaiseAndSetIfChanged(ref neighborhood, value);
    }

    private InclusiveValueRange<int> range;
    public InclusiveValueRange<int> Range
    {
        get => range;
        set
        {
            int upper = CostRange.ReturnInRange(value.UpperValueOfRange);
            int lower = CostRange.ReturnInRange(value.LowerValueOfRange);
            this.RaiseAndSetIfChanged(ref range, (upper, lower));
        }
    }

    public IReadOnlyCollection<Neighborhoods> AllowedNeighborhoods { get; }

    public IReadOnlyCollection<SmoothLevels> AllowedLevels { get; }

    public ReactiveCommand<Unit, Unit> AssembleGraphCommand { get; }

    public GraphAssembleViewModel(
        IGraphRequestService<GraphVertexModel> service,
        IGraphAssemble<GraphVertexModel> graphAssemble,
        ISmoothLevelFactory smoothLevelFactory,
        INeighborhoodLayerFactory neighborFactory,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger,
        ILog logger) : base(logger)
    {
        this.service = service;
        this.messenger = messenger;
        this.graphAssemble = graphAssemble;
        this.neighborFactory = neighborFactory;
        this.smoothLevelFactory = smoothLevelFactory;
        AllowedNeighborhoods = neighborFactory.AvailableNeighborhoods;
        AllowedLevels = smoothLevelFactory.AvailableLevels;
        AssembleGraphCommand = ReactiveCommand.CreateFromTask(CreateGraph, CanExecute());
    }

    private IObservable<bool> CanExecute()
    {
        return this.WhenAnyValue(
            x => x.Width,
            x => x.Length,
            x => x.Obstacles,
            x => x.Name,
            x => x.Range,
            (x, y, z, a, r) => x > 0 && y > 0
                && z >= 0 && !string.IsNullOrEmpty(a)
                && r.UpperValueOfRange >= CostRange.LowerValueOfRange
                && r.UpperValueOfRange <= CostRange.UpperValueOfRange
                && r.LowerValueOfRange >= CostRange.LowerValueOfRange
                && r.LowerValueOfRange <= CostRange.UpperValueOfRange
                && r.UpperValueOfRange > r.LowerValueOfRange
        );
    }

    private async Task CreateGraph()
    {
        await ExecuteSafe(async token =>
        {
            var graph = await AssembleGraph(token)
                .ConfigureAwait(false);
            var request = CreateRequest(graph);
            var graphModel = await service
                .CreateGraphAsync(request, token)
                .ConfigureAwait(false);
            var info = graphModel.ToGraphInformationModel().ToGraphInfo();
            messenger.Send(new GraphsCreatedMessage(info));
        }).ConfigureAwait(false);
    }

    private async Task<IGraph<GraphVertexModel>> AssembleGraph(CancellationToken token)
    {
        int[] param = Depth > 0 
            ? [Width, Length, Depth] 
            : [Width, Length];
        var graph = graphAssemble.AssembleGraph(param);
        graph.CostRange = Range;
        await GetLayers().OverlayAsync(graph, token).ConfigureAwait(false);
        return graph;
    }

    private CreateGraphRequest<GraphVertexModel> CreateRequest(IGraph<GraphVertexModel> graph)
    {
        return new CreateGraphRequest<GraphVertexModel>()
        {
            Graph = graph,
            Name = Name,
            Neighborhood = Neighborhood,
            SmoothLevel = SmoothLevel
        };
    }

    private Layers GetLayers()
    {
        var costLayer = new VertexCostLayer(range
            => new VertexCost(Random.Shared.Next(
                range.LowerValueOfRange,
                range.UpperValueOfRange + 1)));
        var obstacleLayer = new ObstacleLayer(Obstacles);
        var smoothLayer = smoothLevelFactory.Create(SmoothLevel);
        var neighborhoodLayer = neighborFactory.Create(Neighborhood);
        return new(neighborhoodLayer, costLayer, obstacleLayer, smoothLayer);
    }

    public void Dispose()
    {
        AssembleGraphCommand.Dispose();
    }
}
