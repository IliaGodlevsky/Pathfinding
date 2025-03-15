using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Messages.ViewModel;

internal sealed record class IsVertexInRangeRequest(GraphVertexModel Vertex)
{
    public bool IsInRange { get; set; }
}
