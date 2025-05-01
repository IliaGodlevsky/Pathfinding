using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel.Requests;

internal sealed class IsVertexInRangeRequestMessage(GraphVertexModel vertex)
    : RequestMessage<bool>
{
    public GraphVertexModel Vertex { get; } = vertex;
}
