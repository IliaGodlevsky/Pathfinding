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
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels;

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
        AllowedNeighborhoods = neighborFactory.Allowed;
        AllowedLevels = smoothLevelFactory.Allowed;
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
            var layers = GetLayers();
            var graph = graphAssemble.AssembleGraph([Width, Length]);
            graph.CostRange = Range;
            await layers.OverlayAsync(graph, token).ConfigureAwait(false);
            var request = new CreateGraphRequest<GraphVertexModel>()
            {
                Graph = graph,
                Name = Name,
                Neighborhood = Neighborhood,
                SmoothLevel = SmoothLevel
            }; 
            var graphModel = await service
                .CreateGraphAsync(request, token)
                .ConfigureAwait(false);
            var info = graphModel.ToGraphInformationModel().ToGraphInfo();
            messenger.Send(new GraphsCreatedMessage(info));
        }).ConfigureAwait(false);
    }

    private Layers GetLayers()
    {
        var costLayer = new VertexCostLayer(range
            => new VertexCost(Random.Shared.Next(
                range.LowerValueOfRange,
                range.UpperValueOfRange + 1)));
        var obstacleLayer = new ObstacleLayer(Obstacles);
        var smoothLayer = smoothLevelFactory.CreateLayer(SmoothLevel);
        var neighborhoodLayer = neighborFactory.CreateNeighborhoodLayer(Neighborhood);
        return new(neighborhoodLayer, costLayer, obstacleLayer, smoothLayer);
    }

    public void Dispose()
    {
        AssembleGraphCommand.Dispose();
    }
}
