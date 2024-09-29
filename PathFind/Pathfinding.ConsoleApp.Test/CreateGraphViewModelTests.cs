﻿using Autofac.Extras.Moq;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.ConsoleApp.Messages.ViewModel;
using Pathfinding.ConsoleApp.Model;
using Pathfinding.ConsoleApp.ViewModel;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Infrastructure.Data.Pathfinding.Factories;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.TestUtils.Attributes;
using System.Reactive.Linq;

namespace Pathfinding.ConsoleApp.Test
{
    [TestFixture, UnitTest]
    public class CreateGraphViewModelTests
    {
        [Test]
        public async Task CreateGraphCommand_FullData_ShouldCreate()
        {
            using var mock = AutoMock.GetLoose();
            mock.Mock<IGraphAssemble<VertexModel>>()
                .Setup(x => x.AssembleGraph(It.IsAny<IReadOnlyList<int>>()))
                .Returns(Graph<VertexModel>.Empty);
            var viewModel = mock.Create<CreateGraphViewModel>();
            viewModel.NeighborhoodFactory = new MooreNeighborhoodFactory();
            viewModel.Name = "Test";
            viewModel.Length = 1;
            viewModel.SmoothLevel = 0;
            viewModel.Width = 1;
            viewModel.Obstacles = 1;

            await viewModel.CreateCommand.Execute();

            Assert.Multiple(() =>
            {
                mock.Mock<IRequestService<VertexModel>>()
                    .Verify(service => service.CreateGraphAsync(
                        It.IsAny<CreateGraphRequest<VertexModel>>(),
                        It.IsAny<CancellationToken>()), Times.Once);
                mock.Mock<IMessenger>()
                    .Verify(x => x.Send(
                        It.IsAny<GraphCreatedMessage>(),
                        It.IsAny<IsAnyToken>()), Times.Once);
            });
        }

        [Test]
        public async Task CanExecute_ValidData_ShouldReturnTrue()
        {
            using var mock = AutoMock.GetLoose();
            var viewModel = mock.Create<CreateGraphViewModel>();

            viewModel.NeighborhoodFactory = new MooreNeighborhoodFactory();
            viewModel.Name = "Test";
            viewModel.Length = 1;
            viewModel.SmoothLevel = 0;
            viewModel.Width = 1;
            viewModel.Obstacles = 1;

            var canExecute = await viewModel.CreateCommand.CanExecute.FirstOrDefaultAsync();

            Assert.That(canExecute, Is.True);
        }

        [Test]
        public async Task CanExecute_DataIsUnset_ShouldReturnFalse()
        {
            using var mock = AutoMock.GetLoose();
            var viewModel = mock.Create<CreateGraphViewModel>();

            var canExecute = await viewModel.CreateCommand.CanExecute.FirstOrDefaultAsync();

            Assert.That(canExecute, Is.False);
        }
    }
}
