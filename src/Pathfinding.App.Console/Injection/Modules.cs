﻿using Autofac;
using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.App.Console.Views;
using Pathfinding.Domain.Core.Enums;
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
using Terminal.Gui;

namespace Pathfinding.App.Console.Injection;

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