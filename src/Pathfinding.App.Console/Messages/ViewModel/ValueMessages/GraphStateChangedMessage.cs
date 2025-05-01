using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class GraphStateChangedMessage((int Id, GraphStatuses Status) state) 
    : ValueChangedMessage<(int Id, GraphStatuses Status)>(state);