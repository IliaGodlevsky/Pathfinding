using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Export;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Infrastructure.Business.Serializers;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Shared.Extensions;
using System.IO;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphExportViewModelTests
{
    [Test]
    public async Task ExportGraphCommand_HasGraphs_ShouldExport()
    {
        var messenger = new StrongReferenceMessenger();
        var optionsMock = CreateOptionsMock();
        var serializerMock = new Mock<Serializer>();

        var models = Generators.GenerateGraphInfos(3).ToArray();

        var histories = Enumerable.Range(1, 5)
            .Select(_ => new PathfindingHistorySerializationModel())
            .ToArray()
            .To(x => new PathfindingHistoriesSerializationModel { Histories = [.. x] });

        optionsMock
            .Setup(x => x.ReadHistoryAsync(
                It.IsAny<ExportOptions>(),
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        using var viewModel = CreateViewModel(messenger, optionsMock, serializerMock: serializerMock);
        viewModel.Option = ExportOptions.WithRuns;

        messenger.Send(new GraphsSelectedMessage(models));

        if (await viewModel.ExportGraphCommand.CanExecute.FirstAsync(value => value))
        {
            await viewModel.ExportGraphCommand.Execute(() => new StreamModel(new MemoryStream(), StreamFormat.Binary));
        }

        Assert.Multiple(() =>
        {
            optionsMock
                .Verify(x => x.ReadHistoryAsync(
                    It.IsAny<ExportOptions>(),
                    It.IsAny<IReadOnlyCollection<int>>(),
                    It.IsAny<CancellationToken>()), Times.Once);

            serializerMock
                .Verify(x => x.SerializeToAsync(
                    histories,
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task ExportGraphCommand_NullStream_ShouldNotExport()
    {
        var messenger = new StrongReferenceMessenger();
        var optionsMock = CreateOptionsMock();
        var serializerMock = new Mock<Serializer>();

        using var viewModel = CreateViewModel(messenger, optionsMock, serializerMock: serializerMock);

        if (await viewModel.ExportGraphCommand.CanExecute.FirstAsync(value => value))
        {
            await viewModel.ExportGraphCommand.Execute(() => StreamModel.Empty);
        }

        Assert.Multiple(() =>
        {
            optionsMock
                .Verify(x => x.ReadHistoryAsync(
                    It.IsAny<ExportOptions>(),
                    It.IsAny<IReadOnlyCollection<int>>(),
                    It.IsAny<CancellationToken>()), Times.Never);

            serializerMock
                .Verify(x => x.SerializeToAsync(
                    It.IsAny<PathfindingHistoriesSerializationModel>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task ExportGraphCommand_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var optionsMock = CreateOptionsMock();
        var serializer = new BinarySerializer<PathfindingHistoriesSerializationModel>();
        var logMock = new Mock<ILog>();

        optionsMock
            .Setup(x => x.ReadHistoryAsync(
                It.IsAny<ExportOptions>(),
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        using var viewModel = CreateViewModel(
            messenger,
            optionsMock,
            logMock.Object,
            serializer);
        viewModel.Option = ExportOptions.WithRuns;

        messenger.Send(new GraphsSelectedMessage(Generators.GenerateGraphInfos(1).ToArray()));

        await viewModel.ExportGraphCommand.Execute(() => new StreamModel(new MemoryStream(), StreamFormat.Binary));

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    private static GraphExportViewModel CreateViewModel(
        StrongReferenceMessenger messenger,
        Mock<IReadHistoryOptions> optionsMock,
        ILog? logger = null,
        Serializer? serializer = null,
        Mock<Serializer>? serializerMock = null)
    {
        var metadata = new Dictionary<string, object>
        {
            { MetadataKeys.ExportFormat, StreamFormat.Binary },
            { MetadataKeys.Order, 1 }
        };

        var serializers = serializer is not null
            ? new[] { new Meta<Serializer>(serializer, metadata) }
            : new[] { new Meta<Serializer>(serializerMock!.Object, metadata) };

        return new GraphExportViewModel(
            optionsMock.Object,
            messenger,
            serializers,
            logger ?? Mock.Of<ILog>());
    }

    private static Mock<IReadHistoryOptions> CreateOptionsMock()
    {
        var optionsMock = new Mock<IReadHistoryOptions>();
        optionsMock
            .SetupGet(x => x.Allowed)
            .Returns(new[] { ExportOptions.WithRuns });
        return optionsMock;
    }
}
