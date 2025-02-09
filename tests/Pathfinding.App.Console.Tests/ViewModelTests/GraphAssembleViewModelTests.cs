using Autofac.Extras.Moq;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel;
using Pathfinding.App.Console.Model;
using Pathfinding.App.Console.ViewModel;
using Pathfinding.ConsoleApp.Tests;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests
{
    [Category("Unit")]
    internal class GraphAssembleViewModelTests
    {
        [Test]
        public async Task CreateCommand_ValidInputs_ShouldCreateValidGraph()
        {
            using var mock = AutoMock.GetLoose();

            mock.Mock<IGraphAssemble<GraphVertexModel>>()
                .Setup(x => x.AssembleGraph(It.IsAny<IReadOnlyList<int>>()))
                .Returns(Graph<GraphVertexModel>.Empty);
            mock.Mock<IRequestService<GraphVertexModel>>()
                .Setup(x => x.CreateGraphAsync(
                    It.IsAny<CreateGraphRequest<GraphVertexModel>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new GraphModel<GraphVertexModel>()
                {
                    Id = 1,
                    Name = "Demo",
                    SmoothLevel = Domain.Core.SmoothLevels.No,
                    Neighborhood = Domain.Core.Neighborhoods.VonNeumann,
                    Status = Domain.Core.GraphStatuses.Editable,
                    Vertices = [],
                    DimensionSizes = [15, 15]
                }));

            var viewModel = mock.Create<GraphAssembleViewModel>();
            viewModel.SmoothLevel = Domain.Core.SmoothLevels.No;
            viewModel.Length = 15;
            viewModel.Width = 15;
            viewModel.Neighborhood = Domain.Core.Neighborhoods.VonNeumann;
            viewModel.Obstacles = 10;
            viewModel.Name = "Demo";

            var command = viewModel.AssembleGraphCommand;
            bool canExecute = await command.CanExecute.FirstOrDefaultAsync();
            if (canExecute)
            {
                await command.Execute();
            }

            Assert.Multiple(() =>
            {
                Assert.That(canExecute, Is.True);
                mock.Mock<IRequestService<GraphVertexModel>>()
                    .Verify(x => x.CreateGraphAsync(
                        It.IsAny<CreateGraphRequest<GraphVertexModel>>(),
                        It.IsAny<CancellationToken>()), Times.Once);
                mock.Mock<IMessenger>().Verify(x => x.Send(
                    It.IsAny<GraphCreatedMessage>(),
                    It.IsAny<IsAnyToken>()), Times.Once);
                mock.Mock<IGraphAssemble<GraphVertexModel>>()
                    .Verify(x => x.AssembleGraph(
                        It.IsAny<IReadOnlyList<int>>()), Times.Once);
            });
        }
    }
}
