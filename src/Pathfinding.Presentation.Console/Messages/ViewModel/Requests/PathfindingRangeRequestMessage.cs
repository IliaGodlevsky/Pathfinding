using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.Requests;

internal sealed class PathfindingRangeRequestMessage
    : RequestMessage<GraphVertexModel[]>;
