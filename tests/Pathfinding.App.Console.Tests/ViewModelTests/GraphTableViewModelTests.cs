using Autofac.Extras.Moq;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using System.Reactive.Linq;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

namespace Pathfinding.App.Console.Tests.ViewModelTests
{
    [Category("Unit")]
    internal sealed class GraphTableViewModelTests
    {
        [Test]
        public async Task LoadGraphsCommand_ShouldAddGraphs()
        {
            using var mock = AutoMock.GetLoose();

            IReadOnlyCollection<GraphInformationModel> graphs =
                [.. Enumerable.Range(1, 5).Select(x => new GraphInformationModel
                {
                    Id = x,
                    Dimensions = []
                })];
            mock.Mock<IRequestService<GraphVertexModel>>()
                .Setup(x => x.ReadAllGraphInfoAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(graphs));

            var viewModel = mock.Create<GraphTableViewModel>();

            await viewModel.LoadGraphsCommand.Execute();

            Assert.Multiple(() =>
            {
                mock.Mock<IRequestService<GraphVertexModel>>()
                    .Verify(x => x.ReadAllGraphInfoAsync(
                        It.IsAny<CancellationToken>()), Times.Once);
                Assert.That(viewModel.Graphs, Has.Count.EqualTo(graphs.Count));
            });
        }

        [Test]
        public async Task LoadGraphsCommand_ThrowsException_ShouldLogError()
        {
            using var mock = AutoMock.GetLoose();

            mock.Mock<IRequestService<GraphVertexModel>>()
                 .Setup(x => x.ReadAllGraphInfoAsync(It.IsAny<CancellationToken>()))
                 .Throws(new Exception());

            var viewModel = mock.Create<GraphTableViewModel>();

            await viewModel.LoadGraphsCommand.Execute();

            mock.Mock<ILog>()
                .Verify(x => x.Error(
                    It.IsAny<Exception>(),
                    It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ActivateGraphCommand_ShouldSendMessage()
        {
            using var mock = AutoMock.GetLoose();

            var graph = new GraphModel<GraphVertexModel>()
            {
                Id = 1,
                Name = "Test",
                Vertices = [],
                DimensionSizes = []
            };

            mock.Mock<IRequestService<GraphVertexModel>>()
                .Setup(x => x.ReadGraphAsync(
                    It.Is<int>(z => z == 1),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(graph));
            mock.Mock<IMessenger>()
                .Setup(x => x.Send(It.IsAny<AsyncGraphActivatedMessage>(),
                    It.IsAny<IsAnyToken>()))
                .Returns<AsyncGraphActivatedMessage, object>((x, o) => x)
                .Callback<AsyncGraphActivatedMessage, object>((m, t) => m.SetCompleted(true));

            var viewModel = mock.Create<GraphTableViewModel>();

            await viewModel.ActivateGraphCommand.Execute(1);

            Assert.Multiple(() =>
            {
                mock.Mock<IRequestService<GraphVertexModel>>()
                    .Verify(x => x.ReadGraphAsync(
                        It.Is<int>(z => z == 1),
                        It.IsAny<CancellationToken>()), Times.Once);
                mock.Mock<IMessenger>()
                    .Verify(x => x.Send(
                        It.IsAny<AsyncGraphActivatedMessage>(),
                        It.IsAny<IsAnyToken>()), Times.Exactly(2));
                mock.Mock<IMessenger>()
                    .Verify(x => x.Send(
                        It.IsAny<GraphActivatedMessage>(),
                        It.IsAny<IsAnyToken>()), Times.Exactly(2));
            });
        }

        [Test]
        public async Task ActivateGraphCommand_ThrowException_ShouldLogError()
        {
            using var mock = AutoMock.GetLoose();

            mock.Mock<IRequestService<GraphVertexModel>>()
               .Setup(x => x.ReadGraphAsync(
                   It.IsAny<int>(),
                   It.IsAny<CancellationToken>()))
               .Throws(new Exception());

            var viewModel = mock.Create<GraphTableViewModel>();

            await viewModel.ActivateGraphCommand.Execute(new());

            mock.Mock<ILog>()
                .Verify(x => x.Error(
                    It.IsAny<Exception>(),
                    It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SelectedGraphsCommand_ShouldSendMessage()
        {
            using var mock = AutoMock.GetLoose();

            var models = Enumerable.Range(1, 5).ToArray();

            var viewModel = mock.Create<GraphTableViewModel>();

            await viewModel.SelectGraphsCommand.Execute(models);

            mock.Mock<IMessenger>()
                .Verify(x => x.Send(
                    It.IsAny<GraphsSelectedMessage>(),
                    It.IsAny<IsAnyToken>()), Times.Once);
        }
    }
}
