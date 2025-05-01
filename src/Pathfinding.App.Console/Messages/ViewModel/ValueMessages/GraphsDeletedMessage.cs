using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphsDeletedMessage(int[] graphsIds) 
    : ValueChangedMessage<int[]>(graphsIds);