using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.App.Console.Messages.ViewModel.Requests;

internal sealed class PathfindingRangeRequestMessage 
    : RequestMessage<Coordinate[]>;
