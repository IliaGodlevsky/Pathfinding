using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using System.Reactive.Linq;
using Serializer = Pathfinding.Service.Interface.ISerializer<Pathfinding.Service.Interface.Models.Serialization.PathfindingHistoriesSerializationModel>;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphImportViewModelTests
{
    [Test]
    public async Task ImportGraphCommand_ShouldDeserializeAndPublishMessageAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var serializerMock = new Mock<Serializer>();
        var logMock = new Mock<ILog>();

        var serializationModel = new PathfindingHistoriesSerializationModel
        {
            Histories =
            [
                new PathfindingHistorySerializationModel
                {
                    Graph = new GraphSerializationModel
                    {
                        Name = "Imported",
                        Neighborhood = Neighborhoods.Moore,
                        Status = GraphStatuses.Editable,
                        SmoothLevel = SmoothLevels.No,
                        DimensionSizes = []
                    }
                }
            ]
        };

        var created = new[]
        {
            new PathfindingHistoryModel<GraphVertexModel>
            {
                Graph = new GraphModel<GraphVertexModel>
                {
                    Id = 5,
                    Name = "Imported",
                    Neighborhood = Neighborhoods.Moore,
                    Status = GraphStatuses.Editable,
                    SmoothLevel = SmoothLevels.No,
                    Vertices = [],
                    DimensionSizes = []
                }
            }
        };

        serializerMock
            .Setup(x => x.DeserializeFromAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializationModel);

        serviceMock
            .Setup(x => x.CreatePathfindingHistoriesAsync(
                It.IsAny<IEnumerable<PathfindingHistorySerializationModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var serializerMeta = new[]
        {
            new Autofac.Features.Metadata.Meta<Serializer>(
                serializerMock.Object,
                new Dictionary<string, object>
                {
                    [Pathfinding.App.Console.Injection.MetadataKeys.Order] = 1,
                    [Pathfinding.App.Console.Injection.MetadataKeys.ExportFormat] = StreamFormat.Json
                })
        };

        var viewModel = new GraphImportViewModel(
            serviceMock.Object,
            messenger,
            serializerMeta,
            logMock.Object);

        GraphsCreatedMessage createdMessage = null;
        messenger.Register<GraphsCreatedMessage>(this, (_, msg) => createdMessage = msg);

        await viewModel.ImportGraphCommand.Execute(() =>
            new StreamModel(new MemoryStream([1, 2, 3]), StreamFormat.Json));

        Assert.Multiple(() =>
        {
            serializerMock.Verify(x => x.DeserializeFromAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()), Times.Once);
            serviceMock.Verify(x => x.CreatePathfindingHistoriesAsync(
                It.IsAny<IEnumerable<PathfindingHistorySerializationModel>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(createdMessage, Is.Not.Null);
            logMock.Verify(x => x.Info(It.IsAny<string>()), Times.Once);
        });
    }

    [Test]
    public async Task ImportGraphCommand_EmptyStream_ShouldNotCallServiceAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var serializerMock = new Mock<Serializer>();

        var serializerMeta = new[]
        {
            new Autofac.Features.Metadata.Meta<Serializer>(
                serializerMock.Object,
                new Dictionary<string, object>
                {
                    [Pathfinding.App.Console.Injection.MetadataKeys.Order] = 1,
                    [Pathfinding.App.Console.Injection.MetadataKeys.ExportFormat] = StreamFormat.Json
                })
        };

        var viewModel = new GraphImportViewModel(
            serviceMock.Object,
            messenger,
            serializerMeta,
            Mock.Of<ILog>());

        await viewModel.ImportGraphCommand.Execute(() => StreamModel.Empty);

        serviceMock.Verify(x => x.CreatePathfindingHistoriesAsync(
            It.IsAny<IEnumerable<PathfindingHistorySerializationModel>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
