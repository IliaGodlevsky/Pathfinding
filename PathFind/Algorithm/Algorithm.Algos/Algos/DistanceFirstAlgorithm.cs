﻿using Algorithm.Base;
using Algorithm.Interfaces;
using Algorithm.Realizations.Heuristic;
using GraphLib.Interfaces;

namespace Algorithm.Algos.Algos
{
    public class DistanceFirstAlgorithm : GreedyAlgorithm
    {
        public DistanceFirstAlgorithm(IGraph graph,
            IIntermediateEndPoints endPoints, IHeuristic heuristic)
            : base(graph, endPoints)
        {
            this.heuristic = heuristic;
        }

        public DistanceFirstAlgorithm(IGraph graph, IIntermediateEndPoints endPoints)
            : this(graph, endPoints, new EuclidianDistance())
        {

        }

        protected override double GreedyHeuristic(IVertex vertex)
        {
            return heuristic.Calculate(vertex, endPoints.Target);
        }

        private readonly IHeuristic heuristic;
    }
}
