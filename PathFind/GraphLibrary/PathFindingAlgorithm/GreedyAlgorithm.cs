﻿using System;
using System.Linq;
using GraphLibrary.Extensions.SystemTypeExtensions;
using GraphLibrary.Extensions.CustomTypeExtensions;
using GraphLibrary.Vertex.Interface;
using GraphLibrary.PathFindingAlgorithm.Interface;
using System.Collections.Generic;
using GraphLibrary.Graphs;
using GraphLibrary.Graphs.Interface;
using GraphLibrary.EventArguments;

namespace GraphLibrary.PathFindingAlgorithm
{
    /// <summary>
    /// Greedy algorithm. Each step looks for the best vertex and visits it
    /// </summary>
    public class GreedyAlgorithm : IPathFindingAlgorithm
    {
        public event AlgorithmEventHanlder OnStarted;
        public event Action<IVertex> OnVertexVisited;
        public event AlgorithmEventHanlder OnFinished;

        /// <summary>
        /// A function that selects the best vertex on the step
        /// </summary>
        public Func<IVertex, double> GreedyFunction { private get; set; }

        public IGraph Graph { get; set; }

        public GreedyAlgorithm()
        {
            Graph = NullGraph.Instance;
            visitedVerticesStack = new Stack<IVertex>();
        }

        public IEnumerable<IVertex> FindPath()
        {
            OnStarted?.Invoke(this, new AlgorithmEventArgs(Graph));
            var currentVertex = Graph.Start;
            while (!currentVertex.IsEnd)
            {
                var temp = currentVertex;
                currentVertex = GoNextVertex(currentVertex);
                if (!currentVertex.IsIsolated())
                {
                    currentVertex.IsVisited = true;
                    OnVertexVisited?.Invoke(currentVertex);
                    visitedVerticesStack.Push(currentVertex);
                    currentVertex.ParentVertex = temp;
                }
                else
                    currentVertex = visitedVerticesStack.PopOrNullVertex();
            }
            OnFinished?.Invoke(this, new AlgorithmEventArgs(Graph));
            return this.GetFoundPath();
        }

        private IVertex GoNextVertex(IVertex vertex)
        {
            var neighbours = vertex.GetUnvisitedNeighbours().ToList();
            return neighbours.FindOrNullVertex(vert => GreedyFunction(vert) == neighbours.Min(GreedyFunction));
        }

        private readonly Stack<IVertex> visitedVerticesStack;
    }
}
