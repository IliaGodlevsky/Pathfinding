﻿using Algorithm.Infrastructure.EventArguments;
using Common.Extensions.EnumerableExtensions;
using GraphLib.Interfaces;
using System.Collections.Generic;

namespace Algorithm.Base
{
    public abstract class WaveAlgorithm<TStorage> : PathfindingAlgorithm<TStorage>
        where TStorage : IEnumerable<IVertex>, new()
    {
        protected WaveAlgorithm(IEndPoints endPoints)
            : base(endPoints)
        {
        }

        protected override void PrepareForSubPathfinding(Range range)
        {
            base.PrepareForSubPathfinding(range);
            VisitCurrentVertex();
        }

        protected override void VisitCurrentVertex()
        {
            visited.Add(CurrentVertex);
            RaiseVertexVisited(new AlgorithmEventArgs(CurrentVertex));
        }

        protected abstract void RelaxNeighbours(IReadOnlyCollection<IVertex> vertex);

        protected override void InspectVertex(IVertex vertex)
        {
            var neighbours = GetUnvisitedNeighbours(vertex);
            neighbours.ForEach(Enqueued);
            RelaxNeighbours(neighbours);
        }
    }
}