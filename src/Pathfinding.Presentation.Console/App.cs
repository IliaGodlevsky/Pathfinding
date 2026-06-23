using Autofac;
using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Data.InMemory;
using Pathfinding.Data.Sqlite;
using Pathfinding.Domain.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Logging.Interface;
using Pathfinding.Logging.Loggers;
using Pathfinding.Presentation.Console.Export;
using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Presentation.Console.Factories.Algos;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels;
using Pathfinding.Presentation.Console.Views;
using Pathfinding.Serialization;
using Pathfinding.Serialization.Models;
using Pathfinding.Serialization.Services;
using Pathfinding.Service;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Algorithms.Heuristics;
using Pathfinding.Service.Algorithms.StepRules;
using Pathfinding.Service.Commands;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Layers;
using Pathfinding.Service.Services;
using ReactiveUI.Builder;
using Terminal.Gui;
using Attribute = System.Attribute;

namespace Pathfinding.Presentation.Console;

internal static class App
{
    public static ContainerBuilder AddSqlite(this ContainerBuilder builder)
    {
        builder.RegisterInstance(new SqliteUnitOfWorkFactory(Settings.Default.ConnectionString)).As<IUnitOfWorkFactory>()
            .SingleInstance().OnActivated(args => args.Instance.CreateTables());
        return builder;
    }

    public static ContainerBuilder AddGraphLayers(this ContainerBuilder builder)
    {
        builder.RegisterType<MooreNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.Moore);
        
        builder.RegisterType<DiagonalNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.Diagonal);
        builder.RegisterType<KnightsNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.Knight);

        builder.RegisterInstance(new SmoothLayer(1)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.Low);
        builder.RegisterInstance(new SmoothLayer(2)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.Medium);
        builder.RegisterInstance(new SmoothLayer(4)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.High);
        builder.RegisterInstance(new SmoothLayer(8)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.Extreme);

        return builder;
    }

    public static ContainerBuilder AddTransitPathfindingRangeCommands(this ContainerBuilder builder)
    {
        builder.RegisterType<IncludeTransitVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 6).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ReplaceTransitIsolatedVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 1).Keyed<Command>(KeyFilters.IncludeCommands);
        builder.RegisterType<ExcludeTransitVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
            .WithMetadata(MetadataKeys.Order, 3).Keyed<Command>(KeyFilters.ExcludeCommands);
        return builder;
    }

    public static ContainerBuilder AddDataTransfering(this ContainerBuilder builder)
    {
        builder.RegisterType<DataTransferRequestService<GraphVertexModel>>()
            .As<IDataTransferRequestService<GraphVertexModel>>().SingleInstance();

        builder.RegisterType<JsonSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 3)
            .WithMetadata(MetadataKeys.ExportFormat, SerializationFormat.Json);
        builder.RegisterType<BinarySerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 2)
            .WithMetadata(MetadataKeys.ExportFormat, SerializationFormat.Binary);
        builder.RegisterType<MessagePackSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 5)
            .WithMetadata(MetadataKeys.ExportFormat, SerializationFormat.MessagePack);
        builder.RegisterType<XmlSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 4)
            .WithMetadata(MetadataKeys.ExportFormat, SerializationFormat.Xml);
        builder.RegisterType<BundleSerializer<PathfindingHistoriesSerializationModel>>().SingleInstance()
            .As<Serializer>().WithMetadata(MetadataKeys.Order, 1)
            .WithMetadata(MetadataKeys.ExportFormat, SerializationFormat.Csv);

        builder.RegisterType<ReadGraphOnlyOption>().As<IReadHistoryOption>().SingleInstance()
            .WithMetadata(MetadataKeys.ExportOptions, ExportOptions.GraphOnly);
        builder.RegisterType<ReadGraphsWithRangeOption>().As<IReadHistoryOption>().SingleInstance()
            .WithMetadata(MetadataKeys.ExportOptions, ExportOptions.WithRange);
        builder.RegisterType<ReadGraphWithRunsOptions>().As<IReadHistoryOption>().SingleInstance()
            .WithMetadata(MetadataKeys.ExportOptions, ExportOptions.WithRuns);

        builder.RegisterType<GraphCopyButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 3);
        builder.RegisterType<GraphExportButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 4);
        builder.RegisterType<GraphImportButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 5);

        return builder;
    }

    public static ContainerBuilder AddPathfindingAlgorithms(this ContainerBuilder builder)
    {
        builder.RegisterType<DefaultStepRule>().As<IStepRule>().SingleInstance()
            .WithMetadata(MetadataKeys.StepRule, StepRules.Default);
        builder.RegisterType<LandscapeStepRule>().As<IStepRule>().SingleInstance()
            .WithMetadata(MetadataKeys.StepRule, StepRules.Landscape);

        builder.RegisterType<EuclideanDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Euclidean);
        builder.RegisterType<DiagonalDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Diagonal);
        builder.RegisterType<ChebyshevDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Chebyshev);
        builder.RegisterType<ManhattanDistance>().As<IHeuristic>().SingleInstance()
            .WithMetadata(MetadataKeys.Heuristics, Heuristics.Manhattan);

        builder.RegisterType<AStarAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.AStar)
            .WithMetadata(MetadataKeys.Order, 3).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.All)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<SnakeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Snake)
            .WithMetadata(MetadataKeys.Order, 14).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<CostGreedyAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.CostGreedy)
            .WithMetadata(MetadataKeys.Order, 6).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.StepRule)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<IdaStarAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.IdaStar)
            .WithMetadata(MetadataKeys.Order, 5).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.All)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectLeeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectLee)
            .WithMetadata(MetadataKeys.Order, 9).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<LeeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Lee)
            .WithMetadata(MetadataKeys.Order, 8).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectDijkstraAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectDijkstra)
            .WithMetadata(MetadataKeys.Order, 2).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.StepRule)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectAStarAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectAStar)
            .WithMetadata(MetadataKeys.Order, 4).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.All)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DepthFirstAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.DepthFirst)
            .WithMetadata(MetadataKeys.Order, 12).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DistanceFirstAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.DistanceFirst)
            .WithMetadata(MetadataKeys.Order, 11).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.Heuristics)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AStarGreedyAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.AStarGreedy)
            .WithMetadata(MetadataKeys.Order, 7).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.All)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AStarLeeAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.AStarLee)
            .WithMetadata(MetadataKeys.Order, 10).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.Heuristics)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DepthRandomAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.DepthFirstRandom)
            .WithMetadata(MetadataKeys.Order, 13).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<BidirectRandomAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.BidirectRandom)
            .WithMetadata(MetadataKeys.Order, 16).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<DijkstraAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Dijkstra)
            .WithMetadata(MetadataKeys.Order, 1).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.StepRule)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();

        return builder;
    }

    public static ContainerBuilder AddLogging(this ContainerBuilder builder)
    {
        builder.RegisterType<FileLog>().As<ILog>().SingleInstance();
        builder.RegisterType<MessageBoxLog>().As<ILog>().SingleInstance();

        return builder;
    }

    public static IContainer BuildApp(this ContainerBuilder builder)
    {
        builder.RegisterType<InMemoryUnitOfWorkFactory>().As<IUnitOfWorkFactory>()
            .SingleInstance().IfNotRegistered(typeof(IUnitOfWorkFactory));

        builder.RegisterType<GraphAssemble<GraphVertexModel>>().As<IGraphAssemble<GraphVertexModel>>().SingleInstance();
        builder.RegisterType<GraphAssemble<RunVertexModel>>().As<IGraphAssemble<RunVertexModel>>().SingleInstance();

        builder.RegisterType<VonNeumannNeighborhoodLayer>().Keyed<NeighborhoodLayer>(KeyFilters.Neighborhoods)
            .SingleInstance().WithMetadata(MetadataKeys.Neighborhoods, Neighborhoods.VonNeumann);

        builder.RegisterInstance(new SmoothLayer(0)).AsSelf().SingleInstance()
            .WithMetadata(MetadataKeys.SmoothLevels, SmoothLevels.No);

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

        builder.RegisterType<RandomAlgorithmFactory>().WithMetadata(MetadataKeys.Algorithm, Algorithms.Random)
            .WithMetadata(MetadataKeys.Order, 15).WithMetadata(MetadataKeys.Requirements, AlgorithmRequirements.No)
            .SingleInstance().As<IAlgorithmFactory<PathfindingProcess>>();
        builder.RegisterType<AlgorithmsFactory>().As<IAlgorithmsFactory>().SingleInstance();
        
        builder.RegisterType<HeuristicsFactory>().As<IHeuristicsFactory>().SingleInstance();
        builder.RegisterType<SerializerFactory>().As<ISerializerFactory>().SingleInstance();
        builder.RegisterType<SmoothLevelFactory>().As<ISmoothLevelFactory>().SingleInstance();
        builder.RegisterType<StepRulesFactory>().As<IStepRuleFactory>().SingleInstance();
        builder.RegisterType<NeighborhoodLayerFactory>().As<INeighborhoodLayerFactory>()
            .WithAttributeFiltering().SingleInstance();

        builder.RegisterType<ReadHistoryOptions>().As<IReadHistoryOptions>().SingleInstance();

        builder.RegisterType<GraphRequestService<GraphVertexModel>>().As<IGraphRequestService<GraphVertexModel>>().SingleInstance();
        builder.RegisterType<RangeRequestService<GraphVertexModel>>().As<IRangeRequestService<GraphVertexModel>>().SingleInstance();
        builder.RegisterType<StatisticsRequestService>().As<IStatisticsRequestService>().SingleInstance();
        builder.RegisterType<GraphInfoRequestService>().As<IGraphInfoRequestService>().SingleInstance();

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

        builder.RegisterType<GraphAssembleButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 1);
        builder.RegisterType<GraphUpdateButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 2);
        builder.RegisterType<GraphDeleteButton>().Keyed<Button>(KeyFilters.GraphTableButtons)
            .WithAttributeFiltering().WithMetadata(MetadataKeys.Order, 6);

        builder.RegisterType<RunsTableView>().Keyed<View>(KeyFilters.RunsPanel).WithAttributeFiltering();
        builder.RegisterType<RunsTableButtonsFrame>().Keyed<View>(KeyFilters.RunsPanel).WithAttributeFiltering();

        builder.RegisterType<RunCreateButton>().Keyed<View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();
        builder.RegisterType<RunUpdateButton>().Keyed<View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();
        builder.RegisterType<RunDeleteButton>().Keyed<View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();

        return builder.Build();
    }

    public static ContainerBuilder InitiateApp()
    {
        RxAppBuilder.CreateReactiveUIBuilder().BuildApp();
        Application.Init();
        return new();
    }

    public static void RunApp(this IContainer container)
    {
        using var main = container.Resolve<MainView>();
        Application.Top.Add(main);
        Application.Run(x => true);
    }
}