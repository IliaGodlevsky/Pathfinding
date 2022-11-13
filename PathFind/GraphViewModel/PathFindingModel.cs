﻿using Algorithm.Base;
using Algorithm.Exceptions;
using Algorithm.Extensions;
using Algorithm.Factory.Interface;
using Algorithm.Infrastructure.EventArguments;
using Algorithm.Interfaces;
using Algorithm.NullRealizations;
using Common.Attrbiutes;
using Common.Extensions;
using Common.Extensions.EnumerableExtensions;
using GraphLib.Base.EndPoints;
using GraphLib.Extensions;
using GraphLib.Interfaces;
using Interruptable.EventArguments;
using Pathfinding.Logging.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GraphViewModel
{
    public abstract class PathFindingModel<TVertex>
        where TVertex : IVertex, IVisualizable
    {
        protected readonly BaseEndPoints<TVertex> endPoints;
        protected readonly Stopwatch timer;
        protected readonly ILog log;

        protected PathfindingProcess algorithm;

        protected int visitedVerticesCount;

        protected IGraphPath Path { get; set; } = NullGraphPath.Instance;

        protected IGraph<TVertex> Graph { get; }

        public bool IsVisualizationRequired { get; set; } = true;

        public virtual TimeSpan Delay { get; set; }

        public IAlgorithmFactory<PathfindingProcess> Algorithm { get; set; }

        public IReadOnlyList<IAlgorithmFactory<PathfindingProcess>> Algorithms { get; }

        protected PathFindingModel(BaseEndPoints<TVertex> endPoints,
            IEnumerable<IAlgorithmFactory<PathfindingProcess>> factories, IGraph<TVertex> graph, ILog log)
        {
            this.Graph = graph;
            this.endPoints = endPoints;
            this.log = log;
            Algorithms = factories
                .GroupBy(item => item.GetAttributeOrNull<GroupAttribute>())
                .SelectMany(item => item.OrderByOrderAttribute())
                .ToList();
            timer = new Stopwatch();
            Path = NullGraphPath.Interface;
        }

        public virtual async void FindPath()
        {
            try
            {
                algorithm = Algorithm.Create(endPoints);
                SubscribeOnAlgorithmEvents(algorithm);
                Graph.Refresh();
                endPoints.RestoreCurrentColors();
                Path = await algorithm.FindPathAsync();
                await Path.Select(Graph.Get).VisualizeAsPathAsync();
                SummarizePathfindingResults();
            }
            catch (PathfindingException ex)
            {
                log.Debug(ex.Message);
                SummarizePathfindingResults();
            }
            catch (Exception ex)
            {
                algorithm.Interrupt();
                log.Error(ex);
            }
            finally
            {
                algorithm.Dispose();
            }
        }

        protected virtual void OnVertexVisited(object sender, AlgorithmEventArgs e)
        {
            if (!endPoints.IsEndPoint(e.Current))
            {
                var current = Graph.Get(e.Current.Position);
                current.VisualizeAsVisited();
            }
            visitedVerticesCount++;
        }

        protected virtual void OnVertexVisitedNoVisualization(object sender, AlgorithmEventArgs e)
        {
            visitedVerticesCount++;
        }

        protected virtual void OnVertexEnqueued(object sender, AlgorithmEventArgs e)
        {
            if (!endPoints.IsEndPoint(e.Current))
            {
                var current = Graph.Get(e.Current.Position);
                current.VisualizeAsEnqueued();
            }
        }

        protected virtual void OnAlgorithmInterrupted(object sender, ProcessEventArgs e)
        {
        }

        protected virtual void OnAlgorithmFinished(object sender, ProcessEventArgs e)
        {
        }

        protected virtual void OnAlgorithmStarted(object sender, ProcessEventArgs e)
        {
        }

        protected abstract void SummarizePathfindingResults();

        protected virtual void SubscribeOnAlgorithmEvents(PathfindingProcess algorithm)
        {
            if (IsVisualizationRequired)
            {
                algorithm.VertexEnqueued += OnVertexEnqueued;
                algorithm.VertexVisited += OnVertexVisited;
            }
            else
            {
                algorithm.VertexVisited += OnVertexVisitedNoVisualization;
            }
            algorithm.Finished += OnAlgorithmFinished;
            algorithm.Finished += (s, e) => timer.Stop();
            algorithm.Started += OnAlgorithmStarted;
            algorithm.Started += (s, e) => timer.Restart();
            algorithm.Interrupted += OnAlgorithmInterrupted;
            algorithm.Interrupted += (s, e) => timer.Stop();
            algorithm.Paused += (s, e) => timer.Stop();
            algorithm.Resumed += (s, e) => timer.Start();
        }
    }
}