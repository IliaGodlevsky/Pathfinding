using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms
{
    public abstract class DepthAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange) 
        : PathfindingAlgorithm<Stack<IPathfindingVertex>>(pathfindingRange)
    {
        private IPathfindingVertex PreviousVertex { get; set; } = NullPathfindingVertex.Instance;

        protected abstract IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors);

        protected override void MoveNextVertex()
        {
            var neighbours = GetUnvisitedNeighbours(CurrentVertex);
            RaiseVertexProcessed(CurrentVertex, neighbours);
            CurrentVertex = GetVertex(neighbours);
        }

        protected override void PrepareForSubPathfinding(
            (IPathfindingVertex Source, IPathfindingVertex Target) range)
        {
            base.PrepareForSubPathfinding(range);
            visited.Add(CurrentVertex);
            storage.Push(CurrentVertex);
        }

        protected override void DropState()
        {
            base.DropState();
            storage.Clear();
        }

        protected override void VisitCurrentVertex()
        {
            if (CurrentVertex.Neighbors.Count == 0)
            {
                CurrentVertex = storage.PopOrThrowDeadEndVertexException();
            }
            else
            {
                visited.Add(CurrentVertex);
                storage.Push(CurrentVertex);
                traces[CurrentVertex.Position] = PreviousVertex;
            }
        }

        protected override void InspectCurrentVertex()
        {
            PreviousVertex = CurrentVertex;
        }
    }
}