using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class RunsDeletedMessage(int[] runIds)
    : ValueChangedMessage<int[]>(runIds);
