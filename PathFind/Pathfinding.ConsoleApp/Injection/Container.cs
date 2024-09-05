﻿using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.ConsoleApp.Model;
using Pathfinding.ConsoleApp.Model.Factories;
using Pathfinding.ConsoleApp.View;
using Pathfinding.ConsoleApp.View.ButtonsFrameViews;
using Pathfinding.ConsoleApp.View.GraphCreateViews;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business;
using Pathfinding.Infrastructure.Business.Commands;
using Pathfinding.Infrastructure.Business.Mappings;
using Pathfinding.Infrastructure.Business.Serializers;
using Pathfinding.Infrastructure.Business.Serializers.Decorators;
using Pathfinding.Infrastructure.Data.LiteDb;
using Pathfinding.Infrastructure.Data.Pathfinding.Factories;
using Pathfinding.Logging.Interface;
using Pathfinding.Logging.Loggers;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Commands;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using System.Collections.Generic;

namespace Pathfinding.ConsoleApp.Injection
{
    internal static class Container
    {
        public static IContainer BuildApp()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<GraphFactory<VertexModel>>().As<IGraphFactory<VertexModel>>().SingleInstance();
            builder.RegisterType<VertexModelFactory>().As<IVertexFactory<VertexModel>>().SingleInstance();
            builder.RegisterType<GraphAssemble<VertexModel>>().As<IGraphAssemble<VertexModel>>().SingleInstance();

            builder.RegisterInstance(new[] { ("No", 0), ("Low", 1), ("Medium", 2), ("High", 4) })
                .As<IEnumerable<(string Name, int Level)>>().SingleInstance();
            builder.RegisterInstance(new[] {
                ("Moore", (INeighborhoodFactory)new MooreNeighborhoodFactory()),
                ("Von Neimann", new VonNeumannNeighborhoodFactory())
            }).As<IEnumerable<(string Name, INeighborhoodFactory Factory)>>().SingleInstance();

            builder.Register(_ => new LiteDbInFileUnitOfWorkFactory("pathfinding.litedb")).As<IUnitOfWorkFactory>().SingleInstance();

            builder.RegisterAutoMapper();
            builder.RegisterType<RequestService<VertexModel>>().As<IRequestService<VertexModel>>().SingleInstance();

            builder.RegisterType<IncludeSourceVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<IncludeTargetVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<IncludeTransitVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ReplaceTransitIsolatedVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ReplaceIsolatedSourceVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ReplaceIsolatedTargetVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.IncludeCommands);
            builder.RegisterType<ExcludeSourceVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.ExcludeCommands);
            builder.RegisterType<ExcludeTargetVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.ExcludeCommands);
            builder.RegisterType<ExcludeTransitVertex<VertexModel>>().SingleInstance().WithAttributeFiltering()
                .Keyed<IPathfindingRangeCommand<VertexModel>>(KeyFilters.ExcludeCommands);

            builder.RegisterType<JsonSerializer<PathfindingHistorySerializationModel>>()
                .As<ISerializer<PathfindingHistorySerializationModel>>().SingleInstance();

            builder.RegisterDecorator<CompressSerializer<PathfindingHistorySerializationModel>,
                ISerializer<PathfindingHistorySerializationModel>>();

            builder.RegisterType<FileLog>().As<ILog>().SingleInstance();
            builder.RegisterType<ConsoleLog>().As<ILog>().SingleInstance();
            builder.RegisterComposite<Logs, ILog>().As<ILog>().SingleInstance();

            builder.RegisterType<WeakReferenceMessenger>().Keyed<IMessenger>(KeyFilters.Views).SingleInstance().WithAttributeFiltering();
            builder.RegisterType<WeakReferenceMessenger>().Keyed<IMessenger>(KeyFilters.ViewModels).SingleInstance().WithAttributeFiltering();

            builder.RegisterViewModels();

            builder.RegisterType<MainView>().AsSelf().WithAttributeFiltering();

            builder.RegisterType<RightPanelView>().Keyed<Terminal.Gui.View>(KeyFilters.MainWindow).WithAttributeFiltering();
            builder.RegisterType<GraphFrameView>().Keyed<Terminal.Gui.View>(KeyFilters.RightPanel).WithAttributeFiltering();
            builder.RegisterType<GraphsTableView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphFrame).WithAttributeFiltering();
            builder.RegisterType<ButtonsFrameView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphFrame).WithAttributeFiltering();
            builder.RegisterType<LoadGraphButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<DeleteGraphButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<SaveGraphButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<NewGraphButton>().Keyed<Terminal.Gui.View>(KeyFilters.GraphTableButtons).WithAttributeFiltering();
            builder.RegisterType<CreateGraphView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphFrame).WithAttributeFiltering();
            builder.RegisterType<GraphNameView>().Keyed<Terminal.Gui.View>(KeyFilters.CreateGraphView).WithAttributeFiltering();
            builder.RegisterType<GraphParametresView>().Keyed<Terminal.Gui.View>(KeyFilters.CreateGraphView).WithAttributeFiltering();
            builder.RegisterType<NeighborhoodFactoryView>().Keyed<Terminal.Gui.View>(KeyFilters.CreateGraphView).WithAttributeFiltering();
            builder.RegisterType<SmoothLevelView>().Keyed<Terminal.Gui.View>(KeyFilters.CreateGraphView).WithAttributeFiltering();
            builder.RegisterType<GraphFieldView>().Keyed<Terminal.Gui.View>(KeyFilters.MainWindow).WithAttributeFiltering();
            builder.RegisterType<GraphRunsView>().Keyed<Terminal.Gui.View>(KeyFilters.RightPanel).WithAttributeFiltering();
            builder.RegisterType<RunsTableView>().Keyed<Terminal.Gui.View>(KeyFilters.GraphRunsView).WithAttributeFiltering();

            return builder.Build();
        }

        private static void RegisterViewModels(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(x => x.Name.EndsWith("ViewModel")).AsSelf()
                .SingleInstance().WithAttributeFiltering();
        }

        private static void RegisterAutoMapper(this ContainerBuilder builder)
        {
            builder.RegisterType<JsonSerializer<IEnumerable<VisitedVerticesModel>>>()
                .As<ISerializer<IEnumerable<VisitedVerticesModel>>>().SingleInstance();
            builder.RegisterType<JsonSerializer<IEnumerable<CoordinateModel>>>()
                .As<ISerializer<IEnumerable<CoordinateModel>>>().SingleInstance();
            builder.RegisterType<JsonSerializer<IEnumerable<int>>>()
                .As<ISerializer<IEnumerable<int>>>().SingleInstance();

            builder.RegisterType<SubAlgorithmsMappingProfile>().As<Profile>().SingleInstance();
            builder.RegisterType<GraphStateMappingProfile>().As<Profile>().SingleInstance();
            builder.RegisterType<GraphMappingProfile<VertexModel>>().As<Profile>().SingleInstance();
            builder.RegisterType<UntitledMappingProfile>().As<Profile>().SingleInstance();
            builder.RegisterType<HistoryMappingProfile<VertexModel>>().As<Profile>().SingleInstance();
            builder.RegisterType<AlgorithmRunMappingProfile>().As<Profile>().SingleInstance();
            builder.RegisterType<VerticesMappingProfile<VertexModel>>().As<Profile>().SingleInstance();
            builder.RegisterType<StatisticsMappingProfile>().As<Profile>().SingleInstance();

            builder.Register(context =>
            {
                var profiles = context.Resolve<IEnumerable<Profile>>();
                var mappingConfig = new MapperConfiguration(c => c.AddProfiles(profiles));
                return mappingConfig.CreateMapper(context.Resolve);
            }).As<IMapper>().SingleInstance();
        }
    }
}
