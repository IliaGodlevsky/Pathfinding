using Autofac;
using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Export;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Factories.Algos;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.App.Console.Views;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business;
using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Commands;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Business.Serializers;
using Pathfinding.Infrastructure.Business.Serializers.Decorators;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Infrastructure.Data.Sqlite;
using Pathfinding.Logging.Interface;
using Pathfinding.Logging.Loggers;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;
using Terminal.Gui;
using Attribute = System.Attribute;

namespace Pathfinding.App.Console.Injection;

internal static class Modules
{
    public static IContainer Build()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<GraphAssemble<GraphVertexModel>>().As<IGraphAssemble<GraphVertexModel>>().SingleInstance();
        builder.RegisterType<GraphAssemble<RunVertexModel>>().As<IGraphAssemble<RunVertexModel>>().SingleInstance();

        builder.RegisterInstance(new SqliteUnitOfWorkFactory(Settings.Default.ConnectionString)).As<IUnitOfWorkFactory>().SingleInstance();

        builder.RegisterType<RequestService<GraphVertexModel>>().As<IRequestService<GraphVertexModel>>().SingleInstance();

        builder.RegisterType<IncludeSourceVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 2).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<IncludeTargetVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 4).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ReplaceIsolatedSourceVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 3).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ReplaceIsolatedTargetVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 5).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ExcludeSourceVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 1).Keyed<Command>(KeyFilters.ExcludeCommands);
        builder.RegisterType<ExcludeTargetVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 2).Keyed<Command>(KeyFilters.ExcludeCommands);
        builder.RegisterType<IncludeTransitVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 6).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ReplaceTransitIsolatedVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 1).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ExcludeTransitVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 3).Keyed<Command>(KeyFilters.ExcludeCommands);

        builder.RegisterType<JsonSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 3)
            .WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Json);
        builder.RegisterType<BinarySerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .Keyed<Serializer>(KeyFilters.Compress).WithMetadata(MetadataKeys.Order, 2)
            .WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Binary);
        builder.RegisterType<XmlSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 4)
            .WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Xml);
        builder.RegisterType<BundleSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 1)
            .WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Csv);
        builder.RegisterDecorator<Serializer>(
            (_, inner) => new CompressSerializer<PathfindingHistoriesSerializationModel>(inner),
            fromKey: KeyFilters.Compress);
        builder.RegisterDecorator<Serializer>(
            (_, _, inner) => new BufferedSerializer<PathfindingHistoriesSerializationModel>(inner),
            condition: ctx => ctx.CurrentInstance is not CompressSerializer<PathfindingHistoriesSerializationModel>);

        builder.RegisterType<DefaultStepRule>().As<IStepRule>().SingleInstance()
            .WithMetadata(MetadataKeys.StepRule, StepRules.Default);
        builder.RegisterType<LandscapeStepRule>().As<IStepRule>().SingleInstance()
            .WithMetadata(MetadataKeys.StepRule, StepRules.Landscape);
        builder.RegisterType<StepRulesFactory>().As<IStepRuleFactory>().SingleInstance();

        builder.RegisterType<EuclideanDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Euclidean);
        builder.RegisterType<DiagonalDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Diagonal);
        builder.RegisterType<ChebyshevDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Chebyshev);
        builder.RegisterType<ManhattanDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Manhattan);
        builder.RegisterType<HeuristicsFactory>().As<IHeuristicsFactory>().SingleInstance();

        builder.RegisterType<MooreNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.Moore);
        builder.RegisterType<VonNeumannNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.VonNeumann);
        builder.RegisterType<DiagonalNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.Diagonal);
        builder.RegisterType<KnightsNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.Knight);
        builder.RegisterType<NeighborhoodLayerFactory>().As<INeighborhoodLayerFactory>()
            .WithAttributeFiltering().SingleInstance();

        builder.RegisterInstance(new SmoothLayer(0)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.No);
        builder.RegisterInstance(new SmoothLayer(1)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.Low);
        builder.RegisterInstance(new SmoothLayer(2)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.Medium);
        builder.RegisterInstance(new SmoothLayer(4)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.High);
        builder.RegisterInstance(new SmoothLayer(8)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.Extreme);
        builder.RegisterType<SmoothLevelFactory>().As<ISmoothLevelFactory>().SingleInstance();

        builder.RegisterType<RandomAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Random)
            .WithMetadata(MetadataKeys.Order, 15).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AStarAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.AStar)
            .WithMetadata(MetadataKeys.Order, 2).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<SnakeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Snake)
            .WithMetadata(MetadataKeys.Order, 14).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<CostGreedyAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.CostGreedy)
            .WithMetadata(MetadataKeys.Order, 6).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectLeeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectLee)
            .WithMetadata(MetadataKeys.Order, 9).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<LeeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Lee)
            .WithMetadata(MetadataKeys.Order, 8).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectDijkstraAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectDijkstra)
            .WithMetadata(MetadataKeys.Order, 3).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectAStarAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectAStar)
            .WithMetadata(MetadataKeys.Order, 4).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DepthFirstAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.DepthFirst)
            .WithMetadata(MetadataKeys.Order, 12).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DistanceFirstAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.DistanceFirst)
            .WithMetadata(MetadataKeys.Order, 11).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AStarGreedyAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.AStarGreedy)
            .WithMetadata(MetadataKeys.Order, 7).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AStarLeeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.AStarLee)
            .WithMetadata(MetadataKeys.Order, 10).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DepthRandomAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.DepthFirstRandom)
            .WithMetadata(MetadataKeys.Order, 13).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectRandomAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectRandom)
            .WithMetadata(MetadataKeys.Order, 16).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DijkstraAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Dijkstra)
            .WithMetadata(MetadataKeys.Order, 1).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        //builder.RegisterType<IDAStarAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.IdaStar)
        //    .WithMetadata(MetadataKeys.Order, 5).SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AlgorithmsFactory>().As<IAlgorithmsFactory>().SingleInstance();

        builder.RegisterType<ReadGraphOnlyOption>().As<IReadHistoryOption>().SingleInstance()
            .WithMetadata(MetadataKeys.ExportOptions, ExportOptions.GraphOnly);
        builder.RegisterType<ReadGraphsWithRangeOption>().As<IReadHistoryOption>().SingleInstance()
            .WithMetadata(MetadataKeys.ExportOptions, ExportOptions.WithRange);
        builder.RegisterType<ReadGraphWithRunsOptions>().As<IReadHistoryOption>().SingleInstance()
            .WithMetadata(MetadataKeys.ExportOptions, ExportOptions.WithRuns);
        builder.RegisterType<ReadHistoryOptions>().As<IReadHistoryOptions>().SingleInstance();

        builder.RegisterType<FileLog>().As<ILog>().SingleInstance();
        builder.RegisterType<MessageBoxLog>().As<ILog>().SingleInstance();
        builder.RegisterComposite<Logs, ILog>().As<ILog>().SingleInstance();

        builder.RegisterType<StrongReferenceMessenger>().Keyed<IMessenger>(KeyFilters.Views)
            .SingleInstance().WithAttributeFiltering();
        builder.RegisterType<StrongReferenceMessenger>().Keyed<IMessenger>(KeyFilters.ViewModels)
            .SingleInstance().WithAttributeFiltering();

        builder.RegisterAssemblyTypes(typeof(ViewModel).Assembly)
            .Where(x => Attribute.IsDefined(x, typeof(ViewModelAttribute)))
            .SingleInstance().AsSelf().AsImplementedInterfaces().WithAttributeFiltering();

        builder.RegisterType<MainView>().AsSelf().WithAttributeFiltering();

        builder.RegisterType<RightPanelView>().Keyed<View>(KeyFilters.MainWindow).WithAttributeFiltering();
        builder.RegisterType<GraphFieldView>().Keyed<View>(KeyFilters.MainWindow).WithAttributeFiltering();
        builder.RegisterType<RunFieldView>().Keyed<View>(KeyFilters.MainWindow).WithAttributeFiltering();
        builder.RegisterType<RunProgressView>().Keyed<View>(KeyFilters.MainWindow).WithAttributeFiltering();

        builder.RegisterType<GraphPanel>().Keyed<View>(KeyFilters.RightPanel).WithAttributeFiltering();
        builder.RegisterType<RunsPanel>().Keyed<View>(KeyFilters.RightPanel).WithAttributeFiltering();

        builder.RegisterType<GraphsTableView>().Keyed<View>(KeyFilters.GraphPanel).WithAttributeFiltering();
        builder.RegisterType<GraphTableButtonsFrame>().Keyed<View>(KeyFilters.GraphPanel).WithAttributeFiltering();
        builder.RegisterType<GraphAssembleView>().Keyed<View>(KeyFilters.GraphPanel).AsSelf().WithAttributeFiltering().SingleInstance();
        builder.RegisterType<GraphUpdateView>().Keyed<View>(KeyFilters.GraphPanel).AsSelf().WithAttributeFiltering().SingleInstance();

        builder.RegisterType<GraphAssembleButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 1);
        builder.RegisterType<GraphUpdateButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 2);
        builder.RegisterType<GraphCopyButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 3);
        builder.RegisterType<GraphExportButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 4);
        builder.RegisterType<GraphImportButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 5);
        builder.RegisterType<GraphDeleteButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 6);

        builder.RegisterType<GraphNameView>().Keyed<View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();
        builder.RegisterType<GraphParametresView>().Keyed<View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();
        builder.RegisterType<GraphNeighborhoodView>().Keyed<View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();
        builder.RegisterType<GraphSmoothLevelView>().Keyed<View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();

        builder.RegisterType<GraphNameUpdateView>().Keyed<View>(KeyFilters.GraphUpdateView).WithAttributeFiltering();
        builder.RegisterType<GraphNeighborhoodUpdateView>().Keyed<View>(KeyFilters.GraphUpdateView).WithAttributeFiltering();

        builder.RegisterType<RunsTableView>().Keyed<View>(KeyFilters.RunsPanel).WithAttributeFiltering();
        builder.RegisterType<RunsTableButtonsFrame>().Keyed<View>(KeyFilters.RunsPanel).WithAttributeFiltering();
        builder.RegisterType<RunCreateView>().Keyed<View>(KeyFilters.RunsPanel).AsSelf().WithAttributeFiltering().SingleInstance();

        builder.RegisterType<RunCreateButton>().Keyed<View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();
        builder.RegisterType<RunUpdateView>().Keyed<View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();
        builder.RegisterType<RunDeleteButton>().Keyed<View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();

        builder.RegisterType<RunsListView>().Keyed<View>(KeyFilters.RunCreateView).WithAttributeFiltering();
        builder.RegisterType<RunParametresView>().Keyed<View>(KeyFilters.RunCreateView).WithAttributeFiltering();

        builder.RegisterType<RunStepRulesView>().Keyed<View>(KeyFilters.RunParametersView).WithAttributeFiltering();
        builder.RegisterType<RunHeuristicsView>().Keyed<View>(KeyFilters.RunParametersView).WithAttributeFiltering();
        builder.RegisterType<RunsPopulateView>().Keyed<View>(KeyFilters.RunParametersView).WithAttributeFiltering();

        return builder.Build();
    }
}