using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphAssembleViewModelTests
{
    [Test]
    public async Task CreateCommand_ValidInputs_ShouldCreateValidGraph()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var assembleMock = new Mock<IGraphAssemble<GraphVertexModel>>();
        var smoothFactoryMock = new Mock<ISmoothLevelFactory>();
        var neighborFactoryMock = new Mock<INeighborhoodLayerFactory>();

        assembleMock
            .Setup(x => x.AssembleGraph(It.IsAny<IReadOnlyList<int>>()))
            .Returns(Graph<GraphVertexModel>.Empty);

        graphServiceMock
            .Setup(x => x.CreateGraphAsync(
                It.IsAny<CreateGraphRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GraphModel<GraphVertexModel>
            {
                Id = 1,
                Name = "Demo",
                SmoothLevel = SmoothLevels.No,
                Neighborhood = Neighborhoods.VonNeumann,
                Status = GraphStatuses.Editable,
                Vertices = [],
                DimensionSizes = [15, 15]
            });

        smoothFactoryMock
            .SetupGet(x => x.Allowed)
            .Returns(Enum.GetValues<SmoothLevels>());
        smoothFactoryMock
            .Setup(x => x.CreateLayer(It.IsAny<SmoothLevels>()))
            .Returns(new SmoothLayer(0));

        neighborFactoryMock
            .SetupGet(x => x.Allowed)
            .Returns(Enum.GetValues<Neighborhoods>());
        neighborFactoryMock
            .Setup(x => x.CreateNeighborhoodLayer(It.IsAny<Neighborhoods>()))
            .Returns(new MooreNeighborhoodLayer());

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            assembleMock,
            smoothFactoryMock,
            neighborFactoryMock);

        viewModel.SmoothLevel = SmoothLevels.No;
        viewModel.Length = 15;
        viewModel.Width = 15;
        viewModel.Neighborhood = Neighborhoods.VonNeumann;
        viewModel.Obstacles = 10;
        viewModel.Name = "Demo";

        GraphsCreatedMessage createdMessage = null;
        messenger.Register<GraphsCreatedMessage>(this, (_, msg) => createdMessage = msg);

        var canExecute = await viewModel.AssembleGraphCommand.CanExecute.FirstAsync(value => value);

        await viewModel.AssembleGraphCommand.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(canExecute, Is.True);
            graphServiceMock
                .Verify(x => x.CreateGraphAsync(
                    It.IsAny<CreateGraphRequest<GraphVertexModel>>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            assembleMock
                .Verify(x => x.AssembleGraph(
                    It.IsAny<IReadOnlyList<int>>()), Times.Once);
            Assert.That(createdMessage, Is.Not.Null);
        });
    }

    private static GraphAssembleViewModel CreateViewModel(StrongReferenceMessenger messenger,
        Mock<IGraphRequestService<GraphVertexModel>> serviceMock,
        Mock<IGraphAssemble<GraphVertexModel>> assembleMock,
        Mock<ISmoothLevelFactory> smoothFactoryMock,
        Mock<INeighborhoodLayerFactory> neighborFactoryMock,
        ILog logger = null)
    {
        return new GraphAssembleViewModel(
            serviceMock.Object,
            assembleMock.Object,
            smoothFactoryMock.Object,
            neighborFactoryMock.Object,
            messenger,
            logger ?? Mock.Of<ILog>());
    }
}
