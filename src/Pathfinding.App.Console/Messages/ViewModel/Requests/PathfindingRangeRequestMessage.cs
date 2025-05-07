using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel.Requests;

internal sealed class PathfindingRangeRequestMessage 
    : RequestMessage<GraphVertexModel[]>;
