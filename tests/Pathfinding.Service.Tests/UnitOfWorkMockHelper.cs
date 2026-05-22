using Autofac.Extras.Moq;
using Moq;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Business.Tests;

internal static class UnitOfWorkMockHelper
{
    internal static Mock<IUnitOfWork> SetupUnitOfWork(AutoMock mock, Action<Mock<IUnitOfWork>> configure)
    {
        var unit = mock.Mock<IUnitOfWork>();
        unit.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        unit.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unit.Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unit.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        configure?.Invoke(unit);
        mock.Mock<IUnitOfWorkFactory>()
            .Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit.Object);
        return unit;
    }
}
