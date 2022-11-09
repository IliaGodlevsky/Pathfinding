﻿using Pathfinding.AlgorithmLib.Core.Interface;
using Pathfinding.AlgorithmLib.Core.Realizations.Algorithms;
using Pathfinding.AlgorithmLib.Core.Realizations.Heuristics;
using Pathfinding.AlgorithmLib.Factory.Attrbiutes;
using Pathfinding.AlgorithmLib.Factory.Interface;
using Pathfinding.GraphLib.Core.Interface;

namespace Pathfinding.AlgorithmLib.Factory
{
    [GreedyGroup]
    public sealed class DistanceFirstAlgorithmFactory : IAlgorithmFactory<DistanceFirstAlgorithm>
    {
        private readonly IHeuristic heuristic;

        public DistanceFirstAlgorithmFactory(IHeuristic heuristic)
        {
            this.heuristic = heuristic;
        }

        public DistanceFirstAlgorithmFactory()
            : this(new EuclidianDistance())
        {

        }

        public DistanceFirstAlgorithm Create(IPathfindingRange endPoints)
        {
            return new DistanceFirstAlgorithm(endPoints, heuristic);
        }

        public override string ToString()
        {
            return "Distance first algorithm";
        }
    }
}