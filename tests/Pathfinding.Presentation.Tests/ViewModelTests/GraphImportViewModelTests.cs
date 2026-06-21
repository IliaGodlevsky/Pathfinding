using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.Domain.Enums;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels;
using Pathfinding.Serialization.Models;
using Pathfinding.Serialization.Services;
using Pathfinding.Service.Interface.Models.Read;
using System.Reactive.Linq;
using Serializer = Pathfinding.Service.Interface.ISerializer<Pathfinding.Serialization.Models.PathfindingHistoriesSerializationModel>;

namespace Pathfinding.Presentation.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphImportViewModelTests
{
    [Test]
    public async Task ImportGraphCommand_ShouldDeserializeAndPublishMessageAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IDataTransferRequestService<GraphVertexModel>>();
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
                It.IsAny<IReadOnlyCollection<PathfindingHistorySerializationModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var serializerMeta = new[]
        {
            new Autofac.Features.Metadata.Meta<Serializer>(
                serializerMock.Object,
                new Dictionary<string, object>
                {
                    [Console.Injection.MetadataKeys.Order] = 1,
                    [Console.Injection.MetadataKeys.ExportFormat] = SerializationFormat.Json
                })
        };

        var viewModel = new GraphImportViewModel(
            serviceMock.Object,
            messenger,
            new SerializerFactory(serializerMeta),
            logMock.Object);

        GraphsCreatedMessage createdMessage = null;
        messenger.Register<GraphsCreatedMessage>(this, (_, msg) => createdMessage = msg);

        await viewModel.ImportGraphCommand.Execute(() =>
            new StreamModel(new MemoryStream([1, 2, 3]), SerializationFormat.Json));

        using (Assert.EnterMultipleScope())
        {
            serializerMock.Verify(x => x.DeserializeFromAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()), Times.Once);
            serviceMock.Verify(x => x.CreatePathfindingHistoriesAsync(
                It.IsAny<IReadOnlyCollection<PathfindingHistorySerializationModel>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(createdMessage, Is.Not.Null);
            logMock.Verify(x => x.Info(It.IsAny<string>()), Times.Once);
        }
    }

    [Test]
    public async Task ImportGraphCommand_EmptyStream_ShouldNotCallServiceAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IDataTransferRequestService<GraphVertexModel>>();
        var serializerMock = new Mock<Serializer>();

        var serializerMeta = new[]
        {
            new Autofac.Features.Metadata.Meta<Serializer>(
                serializerMock.Object,
                new Dictionary<string, object>
                {
                    [Console.Injection.MetadataKeys.Order] = 1,
                    [Console.Injection.MetadataKeys.ExportFormat] = SerializationFormat.Json
                })
        };

        var viewModel = new GraphImportViewModel(
            serviceMock.Object,
            messenger,
            new SerializerFactory(serializerMeta),
            Mock.Of<ILog>());

        await viewModel.ImportGraphCommand.Execute(() => StreamModel.Empty);

        serviceMock.Verify(x => x.CreatePathfindingHistoriesAsync(
            It.IsAny<IReadOnlyCollection<PathfindingHistorySerializationModel>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
