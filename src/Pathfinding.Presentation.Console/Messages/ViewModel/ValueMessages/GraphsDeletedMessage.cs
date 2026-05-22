using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphsDeletedMessage(int[] graphsIds)
    : ValueChangedMessage<int[]>(graphsIds);