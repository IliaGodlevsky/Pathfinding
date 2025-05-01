using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class ObstaclesCountChangedMessage((int GraphId, int Delta) count)
    : ValueChangedMessage<(int GraphId, int Delta)>(count);
