using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Messages.ViewModel;

internal sealed record GraphStateChangedMessage(int Id, GraphStatuses Status);
