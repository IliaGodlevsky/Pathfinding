﻿using Algorithm.Infrastructure.EventArguments;
using Algorithm.Interfaces;
using Algorithm.NullRealizations;
using Algorithm.Realizations.GraphPaths;
using Common.Extensions.EnumerableExtensions;
using GraphLib.Extensions;
using GraphLib.Interfaces;
using NullObject.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Base
{
    public abstract class GreedyAlgorithm : PathfindingAlgorithm
    {
        protected GreedyAlgorithm(IEndPoints endPoints)
           : base(endPoints)
        {
            visitedVerticesStack = new Stack<IVertex>();
        }

        public override sealed IGraphPath FindPath()
        {
            PrepareForPathfinding();
            while (!IsDestination(endPoints))
            {
                WaitResumed();
                PreviousVertex = CurrentVertex;
                CurrentVertex = NextVertex;
                ProcessCurrentVertex();
            }
            CompletePathfinding();

            return CreateGraphPath();
        }

        protected virtual IGraphPath CreateGraphPath()
        {
            return IsTerminatedPrematurely
                ? new GraphPath(parentVertices, endPoints)
                : NullGraphPath.Instance;
        }
        protected abstract double GreedyHeuristic(IVertex vertex);

        protected override void Reset()
        {
            base.Reset();
            visitedVerticesStack.Clear();
        }

        protected override void PrepareForPathfinding()
        {
            base.PrepareForPathfinding();
            CurrentVertex = endPoints.Source;
            visitedVertices.Add(CurrentVertex);
            RaiseVertexVisited(new AlgorithmEventArgs(CurrentVertex));
            visitedVerticesStack.Push(CurrentVertex);
        }
        protected override IVertex NextVertex
        {
            get
            {
                var neighbours = visitedVertices
                    .GetUnvisitedNeighbours(CurrentVertex)
                    .FilterObstacles()
                    .ToArray();
                double leastVertexCost = neighbours.MinOrDefault(GreedyHeuristic);
                bool IsLeastCostVertex(IVertex vertex)
                    => GreedyHeuristic(vertex) == leastVertexCost;
                return neighbours
                    .ForAll(Enqueue)
                    .FirstOrNullVertex(IsLeastCostVertex);
            }
        }

        private void VisitCurrentVertex()
        {
            visitedVertices.Add(CurrentVertex);
            RaiseVertexVisited(new AlgorithmEventArgs(CurrentVertex));
            visitedVerticesStack.Push(CurrentVertex);
            parentVertices.Add(CurrentVertex, PreviousVertex);
        }

        private void Enqueue(IVertex vertex)
        {
            RaiseVertexEnqueued(new AlgorithmEventArgs(vertex));
        }

        private void ProcessCurrentVertex()
        {
            if (CurrentVertex.IsNull())
            {
                CurrentVertex = visitedVerticesStack.PopOrNullVertex();
            }
            else
            {
                VisitCurrentVertex();
            }
        }

        private IVertex PreviousVertex { get; set; }

        private readonly Stack<IVertex> visitedVerticesStack;
    }
}