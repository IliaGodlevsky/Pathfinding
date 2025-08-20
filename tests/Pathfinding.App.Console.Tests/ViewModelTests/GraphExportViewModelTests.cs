using Autofac;
using Autofac.Extras.Moq;
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
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Shared.Extensions;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal class GraphExportViewModelTests
{
    private static async Task ExportGraphCommand_HasGraphs_ShouldExport(Expression<Func<IReadHistoryOptionsFacade, Task<PathfindingHistoriesSerializationModel>>> expression,
        ExportOptions options)
    {
        using var mock = AutoMock.GetLoose();

        var models = Generators.GenerateGraphInfos(3).ToArray();

        var histories
            = Enumerable.Range(1, 5)
                .Select(_ => new PathfindingHistorySerializationModel())
                .ToArray()
                .To(x => new PathfindingHistoriesSerializationModel { Histories = [.. x] });

        mock.Mock<IReadHistoryOptionsFacade>()
            .Setup(expression)
            .Returns(Task.FromResult(histories));

        mock.Mock<IMessenger>().Setup(x => x.Register(
                It.IsAny<object>(),
                It.IsAny<IsAnyToken>(),
                It.IsAny<MessageHandler<object, GraphsSelectedMessage>>()))
            .Callback<object, object, MessageHandler<object, GraphsSelectedMessage>>((r, _, handler)
                => handler(r, new(models)));

        var serializer = mock.Mock<ISerializer<PathfindingHistoriesSerializationModel>>();
        var meta = new Meta<ISerializer<PathfindingHistoriesSerializationModel>>(serializer.Object, new Dictionary<string, object>()
        {
            { MetadataKeys.ExportFormat, StreamFormat.Binary },
            {MetadataKeys.Order, 1 }
        });
        var typedParam = new TypedParameter(typeof(Meta<ISerializer<PathfindingHistoriesSerializationModel>>[]), new[] { meta });

        var viewModel = mock.Create<GraphExportViewModel>(typedParam);
        viewModel.Options = options;

        var command = viewModel.ExportGraphCommand;
        if (await command.CanExecute.FirstOrDefaultAsync())
        {
            await command.Execute(() => new(new MemoryStream(), StreamFormat.Binary));
        }

        Assert.Multiple(() =>
        {
            mock.Mock<IReadHistoryOptionsFacade>()
                .Verify(x => x.ReadHistoryAsync(
                    It.Is<ExportOptions>(y => y == options),
                    It.IsAny<IReadOnlyCollection<int>>()), Times.Once);

            mock.Mock<IMessenger>()
                .Verify(x => x.Register(
                    It.IsAny<object>(),
                    It.IsAny<IsAnyToken>(),
                    It.IsAny<MessageHandler<object, GraphsSelectedMessage>>()), Times.Once);

            serializer.Verify(x => x.SerializeToAsync(
                It.IsAny<PathfindingHistoriesSerializationModel>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task ExportGraphCommand_WithRuns_ShouldExport()
    {
        await ExportGraphCommand_HasGraphs_ShouldExport(x => x.ReadHistoryAsync(
                It.Is<ExportOptions>(y => y == ExportOptions.WithRuns),
                It.IsAny<IReadOnlyCollection<int>>()),
            ExportOptions.WithRuns);
    }

    [Test]
    public async Task ExportGraphCommand_WithRange_ShouldExport()
    {
        await ExportGraphCommand_HasGraphs_ShouldExport(x => x.ReadHistoryAsync(
                It.Is<ExportOptions>(y => y == ExportOptions.WithRange),
                It.IsAny<IReadOnlyCollection<int>>()),
            ExportOptions.WithRange);
    }

    [Test]
    public async Task ExportGraphCommand_GraphOnly_ShouldExport()
    {
        await ExportGraphCommand_HasGraphs_ShouldExport(x => x.ReadHistoryAsync(
                It.Is<ExportOptions>(y => y == ExportOptions.GraphOnly),
                It.IsAny<IReadOnlyCollection<int>>()),
            ExportOptions.GraphOnly);
    }

    [Test]
    public async Task ExportGraphCommand_NullStream_ShouldNotExport()
    {
        using var mock = AutoMock.GetLoose();

        var serializer = mock.Mock<ISerializer<PathfindingHistoriesSerializationModel>>();
        var meta = new Meta<ISerializer<PathfindingHistoriesSerializationModel>>(serializer.Object,
            new Dictionary<string, object>
            {
                { MetadataKeys.ExportFormat, StreamFormat.Binary },
                { MetadataKeys.Order, 1 }
            });
        var typedParam = new TypedParameter(typeof(Meta<ISerializer<PathfindingHistoriesSerializationModel>>[]), new[] { meta });

        var viewModel = mock.Create<GraphExportViewModel>(typedParam);

        var command = viewModel.ExportGraphCommand;
        if (await command.CanExecute.FirstOrDefaultAsync())
        {
            await command.Execute(() => StreamModel.Empty);
        }

        Assert.Multiple(() =>
        {
            mock.Mock<IRequestService<GraphVertexModel>>()
                .Verify(x => x.ReadSerializationHistoriesAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()), Times.Never);

            serializer.Verify(x => x.SerializeToAsync(
                It.IsAny<PathfindingHistoriesSerializationModel>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task ExportGraphCommand_ThrowsException_ShouldLogError()
    {
        using var mock = AutoMock.GetLoose(builder =>
        {
            builder.RegisterType<BinarySerializer<PathfindingHistoriesSerializationModel>>()
                .As<ISerializer<PathfindingHistoriesSerializationModel>>()
                .SingleInstance().WithMetadata(MetadataKeys.ExportFormat, StreamFormat.Binary)
                .WithMetadata(MetadataKeys.Order, 1);
        });

        mock.Mock<IRequestService<GraphVertexModel>>()
            .Setup(x => x.ReadSerializationHistoriesAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Throws(new Exception());

        var viewModel = mock.Create<GraphExportViewModel>();

        var command = viewModel.ExportGraphCommand;
        await command.Execute(() => new(new MemoryStream(), StreamFormat.Binary));

        mock.Mock<ILog>()
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }
}