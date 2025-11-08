using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Export;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface.Models.Serialization;
using System.Reactive.Linq;

using Serializer = Pathfinding.Service.Interface.ISerializer<Pathfinding.Service.Interface.Models.Serialization.PathfindingHistoriesSerializationModel>;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphExportViewModelTests
{
    [Test]
    public async Task ExportGraphCommand_ShouldSerializeHistories()
    {
        var messenger = new StrongReferenceMessenger();
        var optionsMock = new Mock<IReadHistoryOptions>();
        var serializerMock = new Mock<Serializer>();
        var logMock = new Mock<ILog>();

        optionsMock
            .SetupGet(x => x.Allowed)
            .Returns([ExportOptions.GraphOnly, ExportOptions.WithRuns]);

        var histories = new PathfindingHistoriesSerializationModel
        {
            Histories = [new PathfindingHistorySerializationModel()]
        };

        var graphs = Generators.GenerateGraphInfos(3).ToArray();
        var expectedIds = graphs.Skip(1).Select(x => x.Id).ToArray();

        optionsMock
            .Setup(x => x.ReadHistoryAsync(
                It.Is<ExportOptions>(option => option == ExportOptions.WithRuns),
                It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(expectedIds)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        serializerMock
            .Setup(x => x.SerializeToAsync(
                histories,
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var viewModel = CreateViewModel(
            messenger,
            optionsMock,
            [CreateSerializerMeta(StreamFormat.Json, serializerMock)],
            logMock.Object);

        viewModel.Option = ExportOptions.WithRuns;

        messenger.Send(new GraphsSelectedMessage(graphs));
        messenger.Send(new GraphsDeletedMessage([graphs[0].Id]));

        if (await viewModel.ExportGraphCommand.CanExecute.FirstAsync(value => value))
        {
            await viewModel.ExportGraphCommand.Execute(() => new StreamModel(new MemoryStream(), StreamFormat.Json));
        }

        Assert.Multiple(() =>
        {
            optionsMock
                .Verify(x => x.ReadHistoryAsync(
                    It.Is<ExportOptions>(option => option == ExportOptions.WithRuns),
                    It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(expectedIds)),
                    It.IsAny<CancellationToken>()), Times.Once);

            serializerMock
                .Verify(x => x.SerializeToAsync(
                    histories,
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()), Times.Once);

            logMock
                .Verify(x => x.Info(
                    It.Is<string>(message => message == Resource.WasDeletedMsg)), Times.Once);
        });
    }

    [Test]
    public async Task ExportGraphCommand_StreamIsEmpty_ShouldNotSerialize()
    {
        var messenger = new StrongReferenceMessenger();
        var optionsMock = new Mock<IReadHistoryOptions>();
        var serializerMock = new Mock<Serializer>();

        using var viewModel = CreateViewModel(
            messenger,
            optionsMock,
            [CreateSerializerMeta(StreamFormat.Json, serializerMock)]);

        viewModel.Option = ExportOptions.GraphOnly;

        messenger.Send(new GraphsSelectedMessage([.. Generators.GenerateGraphInfos(1)]));

        if (await viewModel.ExportGraphCommand.CanExecute.FirstAsync(value => value))
        {
            await viewModel.ExportGraphCommand.Execute(() => StreamModel.Empty);
        }

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
    }

    [Test]
    public async Task ExportGraphCommand_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var optionsMock = new Mock<IReadHistoryOptions>();
        var serializerMock = new Mock<Serializer>();
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
            [CreateSerializerMeta(StreamFormat.Json, serializerMock)],
            logMock.Object);

        viewModel.Option = ExportOptions.WithRange;

        messenger.Send(new GraphsSelectedMessage([.. Generators.GenerateGraphInfos(1)]));

        if (await viewModel.ExportGraphCommand.CanExecute.FirstAsync(value => value))
        {
            await viewModel.ExportGraphCommand.Execute(() => new StreamModel(new MemoryStream(), StreamFormat.Json));
        }

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    private static GraphExportViewModel CreateViewModel(
        StrongReferenceMessenger messenger,
        Mock<IReadHistoryOptions> optionsMock,
        Meta<Serializer>[] serializers,
        ILog log = null)
    {
        return new GraphExportViewModel(
            optionsMock.Object,
            messenger,
            serializers,
            log ?? Mock.Of<ILog>());
    }

    private static Meta<Serializer> CreateSerializerMeta(
        StreamFormat format,
        Mock<Serializer> serializerMock)
    {
        var metadata = new Dictionary<string, object>
        {
            [MetadataKeys.ExportFormat] = format,
            [MetadataKeys.Order] = 1
        };

        return new Meta<Serializer>(serializerMock.Object, metadata);
    }
}
