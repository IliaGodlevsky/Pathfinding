using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface.Requests.Update
{
    public class UpdateVerticesRequest<T>(int graphId, List<T> vertices)
        where T : IVertex
    {
        public int GraphId { get; } = graphId;

        public List<T> Vertices { get; } = vertices;
    }
}
