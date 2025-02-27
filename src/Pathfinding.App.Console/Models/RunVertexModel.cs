using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using ReactiveUI;

namespace Pathfinding.App.Console.Models
{
    public class RunVertexModel : ReactiveObject, IVertex, IPathfindingVertex
    {
        private bool isObstacle;
        public bool IsObstacle
        {
            get => isObstacle;
            set => this.RaiseAndSetIfChanged(ref isObstacle, value);
        }

        public Coordinate Position { get; set; }

        private bool isVisited;
        public bool IsVisited
        {
            get => isVisited;
            set => this.RaiseAndSetIfChanged(ref isVisited, value);
        }

        private bool isEnqueued;
        public bool IsEnqueued
        {
            get => isEnqueued;
            set => this.RaiseAndSetIfChanged(ref isEnqueued, value);
        }

        private bool isPath;
        public bool IsPath
        {
            get => isPath;
            set => this.RaiseAndSetIfChanged(ref isPath, value);
        }

        private bool isCrossedPath;
        public bool IsCrossedPath
        {
            get => isCrossedPath;
            set => this.RaiseAndSetIfChanged(ref isCrossedPath, value);
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

        public HashSet<RunVertexModel> Neighbors { get; private set; } = [];

        IReadOnlyCollection<IVertex> IVertex.Neighbors
        {
            get => Neighbors;
            set => Neighbors = value.Cast<RunVertexModel>().ToHashSet();
        }

        IReadOnlyCollection<IPathfindingVertex> IPathfindingVertex.Neighbors => Neighbors;

        public bool Equals(IVertex other) => other.IsEqual(this);

        public override bool Equals(object obj) => obj is IVertex vertex && Equals(vertex);

        public override int GetHashCode() => Position.GetHashCode();
    }
}
