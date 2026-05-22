using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphStateChangedMessage((int Id, GraphStatuses Status) state)
    : ValueChangedMessage<(int Id, GraphStatuses Status)>(state);