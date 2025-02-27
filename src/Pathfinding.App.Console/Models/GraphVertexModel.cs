using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using ReactiveUI;

namespace Pathfinding.App.Console.Models
{
    public class GraphVertexModel : ReactiveObject, IVertex, IPathfindingVertex, IEntity<long>
    {
        public long Id { get; set; }

        private bool isObstacle;
        public bool IsObstacle
        {
            get => isObstacle;
            set => this.RaiseAndSetIfChanged(ref isObstacle, value);
        }

        private bool isSource;
        public bool IsSource
        {
            get => isSource;
            set => this.RaiseAndSetIfChanged(ref isSource, value);
        }

        private bool isTarget;
        public bool IsTarget
        {
            get => isTarget;
            set => this.RaiseAndSetIfChanged(ref isTarget, value);
        }

        private bool isTransit;
        public bool IsTransit
        {
            get => isTransit;
            set => this.RaiseAndSetIfChanged(ref isTransit, value);
        }

        private IVertexCost cost;
        public IVertexCost Cost
        {
            get => cost;
            set => this.RaiseAndSetIfChanged(ref cost, value);
        }

        public Coordinate Position { get; set; }

        public HashSet<GraphVertexModel> Neighbors { get; private set; } = [];

        IReadOnlyCollection<IVertex> IVertex.Neighbors
        {
            get => Neighbors;
            set => Neighbors = value.Cast<GraphVertexModel>().ToHashSet();
        }

        IReadOnlyCollection<IPathfindingVertex> IPathfindingVertex.Neighbors => Neighbors;

        public bool Equals(IVertex other) => other.IsEqual(this);

        public override bool Equals(object obj) => obj is IVertex vertex && Equals(vertex);

        public override int GetHashCode() => Position.GetHashCode();
    }
}
