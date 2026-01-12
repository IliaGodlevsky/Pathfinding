namespace Pathfinding.Service.Interface.Requests.Create;

public record CreatePathfindingVertexRequest(
    int GraphId,
    long VertexId,
    int Index);
