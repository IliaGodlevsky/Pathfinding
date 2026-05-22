using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class RunsDeletedMessage(int[] runIds)
    : ValueChangedMessage<int[]>(runIds);
