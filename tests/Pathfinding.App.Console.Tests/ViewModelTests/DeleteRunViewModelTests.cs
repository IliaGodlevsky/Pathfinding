using Autofac.Extras.Moq;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.ConsoleApp.Tests;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests
{
    [Category("Unit")]
    internal class DeleteRunViewModelTests
    {
        [Test]
        public async Task DeleteRuns_SeveralRunIds_ShouldDelete()
        {
            using var mock = AutoMock.GetLoose();
            var runModels = new RunInfoModel[]
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 },
            };

            mock.Mock<IRequestService<GraphVertexModel>>()
                .Setup(x => x.DeleteRunsAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            mock.Mock<IMessenger>().Setup(x => x.Register(
                    It.IsAny<object>(),
                    It.IsAny<IsAnyToken>(),
                    It.IsAny<MessageHandler<object, RunSelectedMessage>>()))
               .Callback<object, object, MessageHandler<object, RunSelectedMessage>>((r, t, handler)
                    => handler(r, new RunSelectedMessage(runModels)));

            var viewModel = mock.Create<RunDeleteViewModel>();

            var command = viewModel.DeleteRunsCommand;
            var canExecute = await command.CanExecute.FirstOrDefaultAsync();
            if (canExecute)
            {
                await command.Execute();
            }

            Assert.Multiple(() =>
            {
                Assert.That(canExecute, Is.True);
                mock.Mock<IRequestService<GraphVertexModel>>()
                    .Verify(x => x.DeleteRunsAsync(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<CancellationToken>()), Times.Once);
                mock.Mock<IMessenger>()
                    .Verify(x => x.Register(
                        It.IsAny<RunDeleteViewModel>(),
                        It.IsAny<IsAnyToken>(),
                        It.IsAny<MessageHandler<object, RunSelectedMessage>>()), Times.Once);
                mock.Mock<IMessenger>()
                    .Verify(x => x.Send(
                        It.Is<RunsDeletedMessage>(x => runModels.Select(x => x.Id).SequenceEqual(x.RunIds)),
                        It.IsAny<IsAnyToken>()), Times.Once);
            });
        }

        [Test]
        public async Task DeletRunCommand_ThrowsException_ShouldLogError()
        {
            using var mock = AutoMock.GetLoose();

            mock.Mock<IRequestService<GraphVertexModel>>()
                .Setup(x => x.DeleteRunsAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var viewModel = mock.Create<RunDeleteViewModel>();

            await viewModel.DeleteRunsCommand.Execute();

            mock.Mock<ILog>()
                .Verify(x => x.Error(
                    It.IsAny<Exception>(),
                    It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task DeleteGraphCommand_NoGraphs_ShouldNotExecute()
        {
            using var mock = AutoMock.GetLoose();

            var viewModel = mock.Create<RunDeleteViewModel>();

            var command = viewModel.DeleteRunsCommand;
            var canExecute = await command.CanExecute.FirstOrDefaultAsync();
            if (canExecute)
            {
                await command.Execute();
            }

            Assert.Multiple(() =>
            {
                Assert.That(canExecute, Is.False);
                mock.Mock<IRequestService<GraphVertexModel>>()
                    .Verify(x => x.DeleteRunsAsync(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<CancellationToken>()), Times.Never);
                mock.Mock<IMessenger>()
                    .Verify(x => x.Send(
                        It.IsAny<RunsDeletedMessage>(),
                        It.IsAny<IsAnyToken>()), Times.Never);
            });
        }
    }
}
