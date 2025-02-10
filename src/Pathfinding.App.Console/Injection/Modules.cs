using Autofac;
using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Model;
using Pathfinding.App.Console.View;
using Pathfinding.App.Console.ViewModel;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business;
using Pathfinding.Infrastructure.Business.Commands;
using Pathfinding.Infrastructure.Business.Serializers;
using Pathfinding.Infrastructure.Business.Serializers.Decorators;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Infrastructure.Data.Sqlite;
using Pathfinding.Logging.Interface;
using Pathfinding.Logging.Loggers;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.App.Console.Injection
{
    internal static class Modules
    {
        public static ILifetimeScope Build()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<GraphAssemble<GraphVertexModel>>().As<IGraphAssemble<GraphVertexModel>>().SingleInstance();
            builder.RegisterType<GraphAssemble<RunVertexModel>>().As<IGraphAssemble<RunVertexModel>>().SingleInstance();

            builder.Register(_ => new SqliteUnitOfWorkFactory(Settings.Default.ConnectionString))
                .As<IUnitOfWorkFactory>().SingleInstance();

            builder.RegisterType<RequestService<GraphVertexModel>>().As<IRequestService<GraphVertexModel>>().SingleInstance();

            builder.RegisterType<IncludeSourceVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 2).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<IncludeTargetVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 4).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ReplaceIsolatedSourceVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 3).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ReplaceIsolatedTargetVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 5).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ExcludeSourceVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 1).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.ExcludeCommands);
            builder.RegisterType<ExcludeTargetVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 2).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.ExcludeCommands);
            builder.RegisterType<IncludeTransitVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 6).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ReplaceTransitIsolatedVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 1).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ExcludeTransitVertex<GraphVertexModel>>().SingleInstance().WithAttributeFiltering()
                .WithMetadata(MetadataKeys.Order, 3).Keyed<IPathfindingRangeCommand<GraphVertexModel>>(KeyFilters.ExcludeCommands);

            builder.RegisterType<JsonSerializer<PathfindingHisotiriesSerializationModel>>()
                .As<ISerializer<PathfindingHisotiriesSerializationModel>>()
                .SingleInstance().WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Json);
            builder.RegisterType<BinarySerializer<PathfindingHisotiriesSerializationModel>>()
                .As<ISerializer<PathfindingHisotiriesSerializationModel>>()
                .SingleInstance().WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Binary);
            builder.RegisterType<XmlSerializer<PathfindingHisotiriesSerializationModel>>()
                .As<ISerializer<PathfindingHisotiriesSerializationModel>>()
                .SingleInstance().WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Xml);
            builder.RegisterGenericDecorator(typeof(BufferedSerializer<>), typeof(ISerializer<>));

            builder.RegisterType<FileLog>().As<ILog>().SingleInstance();
            builder.RegisterType<MessageBoxLog>().As<ILog>().SingleInstance();
            builder.RegisterComposite<Logs, ILog>().As<ILog>().SingleInstance();

            builder.RegisterType<WeakReferenceMessenger>().Keyed<IMessenger>(KeyFilters.Views)
                .SingleInstance().WithAttributeFiltering();
            builder.RegisterType<WeakReferenceMessenger>().Keyed<IMessenger>(KeyFilters.ViewModels)
                .SingleInstance().WithAttributeFiltering();

            builder.RegisterAssemblyTypes(typeof(BaseViewModel).Assembly)
                .SingleInstance().Where(x => x.Name.EndsWith("ViewModel")).AsSelf()
                .AsImplementedInterfaces().WithAttributeFiltering();

            builder.RegisterType<MainView>().AsSelf().WithAttributeFiltering();

            builder.RegisterType<RightPanelView>().Keyed<Terminal.Gui.View>(KeyFilters.MainWindow).WithAttributeFiltering();
            builder.RegisterType<GraphFieldView>().Keyed<Terminal.Gui.View>(KeyFilters.MainWindow).WithAttributeFiltering();
            builder.RegisterType<RunFieldView>().Keyed<Terminal.Gui.View>(KeyFilters.MainWindow).WithAttributeFiltering();
            builder.RegisterType<RunProgressView>().Keyed<Terminal.Gui.View>(KeyFilters.MainWindow).WithAttributeFiltering();

            builder.RegisterType<GraphPanel>().Keyed<Terminal.Gui.View>(KeyFilters.RightPanel).WithAttributeFiltering();
            builder.RegisterType<RunsPanel>().Keyed<Terminal.Gui.View>(KeyFilters.RightPanel).WithAttributeFiltering();

            builder.RegisterType<GraphsTableView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphPanel).WithAttributeFiltering();
            builder.RegisterType<GraphTableButtonsFrame>().Keyed<Terminal.Gui.View>(KeyFilters.GraphPanel).WithAttributeFiltering();
            builder.RegisterType<GraphAssembleView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphPanel).AsSelf().WithAttributeFiltering().SingleInstance();
            builder.RegisterType<GraphUpdateView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphPanel).AsSelf().WithAttributeFiltering().SingleInstance();

            builder.RegisterType<GraphAssembleButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<GraphUpdateButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<GraphCopyButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<GraphExportButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<GraphImportButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<GraphDeleteButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();

            builder.RegisterType<GraphNameView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();
            builder.RegisterType<GraphParametresView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();
            builder.RegisterType<GraphNeighborhoodView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();
            builder.RegisterType<GraphSmoothLevelView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphAssembleView).WithAttributeFiltering();

            builder.RegisterType<GraphNameUpdateView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphUpdateView).WithAttributeFiltering();
            builder.RegisterType<GraphSmoothLevelUpdateView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphUpdateView).WithAttributeFiltering();
            builder.RegisterType<GraphNeighborhoodUpdateView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphUpdateView).WithAttributeFiltering();

            builder.RegisterType<RunsTableView>().Keyed<Terminal.Gui.View>(KeyFilters.RunsPanel).WithAttributeFiltering();
            builder.RegisterType<RunsTableButtonsFrame>().Keyed<Terminal.Gui.View>(KeyFilters.RunsPanel).WithAttributeFiltering();
            builder.RegisterType<RunCreateView>().Keyed<Terminal.Gui.View>(KeyFilters.RunsPanel).AsSelf().WithAttributeFiltering().SingleInstance();

            builder.RegisterType<RunCreateButton>().Keyed<Terminal.Gui.View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();
            builder.RegisterType<RunUpdateView>().Keyed<Terminal.Gui.View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();
            builder.RegisterType<RunDeleteButton>().Keyed<Terminal.Gui.View>(KeyFilters.RunButtonsFrame).WithAttributeFiltering();

            builder.RegisterType<RunsListView>().Keyed<Terminal.Gui.View>(KeyFilters.RunCreateView).WithAttributeFiltering();
            builder.RegisterType<RunParametresView>().Keyed<Terminal.Gui.View>(KeyFilters.RunCreateView).WithAttributeFiltering();

            builder.RegisterType<RunStepRulesView>().Keyed<Terminal.Gui.View>(KeyFilters.RunParametresView).WithAttributeFiltering();
            builder.RegisterType<RunHeuristicsView>().Keyed<Terminal.Gui.View>(KeyFilters.RunParametresView).WithAttributeFiltering();
            builder.RegisterType<RunsPopulateView>().Keyed<Terminal.Gui.View>(KeyFilters.RunParametresView).WithAttributeFiltering();

            return builder.Build();
        }
    }
}