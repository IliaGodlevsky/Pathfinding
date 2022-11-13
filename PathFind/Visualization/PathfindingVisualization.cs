﻿using Algorithm.Base;
using Algorithm.Infrastructure.EventArguments;
using Algorithm.Interfaces;
using Commands.Interfaces;
using Common.Extensions.EnumerableExtensions;
using GraphLib.Extensions;
using GraphLib.Interfaces;
using System;
using System.Linq;
using Visualization.Extensions;
using Visualization.Interfaces;
using Visualization.Realizations;

namespace Visualization
{
    public abstract class PathfindingVisualization<TGraph, TVertex> : IExecutable<IAlgorithm<IGraphPath>>, IVisualizationSlides<IGraphPath>, IVisualizationSlides<IEndPoints>
        where TGraph : IGraph<TVertex>
        where TVertex : IVertex, IVisualizable
    {
        private readonly VisitedVertices<TVertex> visited = new VisitedVertices<TVertex>();
        private readonly EnqueuedVertices<TVertex> enqueued = new EnqueuedVertices<TVertex>();
        private readonly PathVertices<TVertex> path = new PathVertices<TVertex>();
        private readonly IntermediateVertices<TVertex> intermediate = new IntermediateVertices<TVertex>();
        private readonly SourceVertices<TVertex> source = new SourceVertices<TVertex>();
        private readonly TargetVertices<TVertex> target = new TargetVertices<TVertex>();
        private readonly ObstacleVertices<TVertex> obstacles = new ObstacleVertices<TVertex>();
        private readonly IExecutable<IAlgorithm<IGraphPath>> visualizations;
        private readonly IVisualizationSlides<TVertex> visualizationSlides;
        private readonly TGraph graph;

        protected PathfindingVisualization(TGraph graph)
        {
            visualizations = new CompositeVisualization<TGraph, TVertex>(graph) { obstacles, enqueued, visited, path, intermediate, source, target };
            visualizationSlides = new CompositeVisualizationSlides<TVertex> { obstacles, enqueued, visited, path, intermediate, source, target };
            this.graph = graph;
        }

        public void Clear()
        {
            visualizationSlides.Clear();
        }

        public void Remove(IAlgorithm<IGraphPath> algorithm)
        {
            visualizationSlides.Remove(algorithm);
        }

        public void Execute(IAlgorithm<IGraphPath> algorithm)
        {
            visualizations.Execute(algorithm);
        }

        protected virtual void SubscribeOnAlgorithmEvents(PathfindingProcess algorithm)
        {
            algorithm.VertexVisited += OnVertexVisited;
            algorithm.VertexEnqueued += OnVertexEnqueued;
            algorithm.Finished += OnAlgorithmFinished;
            algorithm.Started += OnAlgorithmStarted;
        }

        public void Add(IAlgorithm<IGraphPath> algorithm, IEndPoints endPoints)
        {
            source.Add(algorithm, graph.Get(endPoints.Source.Position));
            target.Add(algorithm, graph.Get(endPoints.Target.Position));
            var intermediates = endPoints.GetIntermediates()
                .Select(item => graph.Get(item.Position));
            intermediate.AddRange(algorithm, intermediates);
        }

        public void Add(IAlgorithm<IGraphPath> algorithm, IGraphPath graphPath)
        {
            var endPoints = source.GetVertices(algorithm)
                .Concat(target.GetVertices(algorithm))
                .Concat(intermediate.GetVertices(algorithm));
            var vertices = graphPath.Select(graph.Get)
                .Where(item => !endPoints.Contains(item));
            path.AddRange(algorithm, vertices);
        }

        protected virtual void OnAlgorithmStarted(object sender, EventArgs e)
        {
            if (sender is IAlgorithm<IGraphPath> algorithm)
            {
                obstacles.AddRange(algorithm, graph.GetObstacles<TGraph, TVertex>());
            }
        }

        protected virtual void OnVertexVisited(object sender, AlgorithmEventArgs e)
        {
            var current = graph.Get(e.Current.Position);
            if (sender is IAlgorithm<IGraphPath> algo)
            {
                visited.Add(algo, current);
            }
        }

        protected virtual void OnVertexEnqueued(object sender, AlgorithmEventArgs e)
        {
            var current = graph.Get(e.Current.Position);
            if (sender is IAlgorithm<IGraphPath> algo)
            {
                enqueued.Add(algo, current);
            }
        }

        protected virtual void OnAlgorithmFinished(object sender, EventArgs e)
        {
            if (sender is IAlgorithm<IGraphPath> algorithm)
            {
                enqueued.RemoveRange(algorithm, visited.GetVertices(algorithm));
            }
        }
    }
}