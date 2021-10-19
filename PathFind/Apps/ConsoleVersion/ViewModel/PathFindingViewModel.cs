﻿using Algorithm.Base;
using Algorithm.Factory;
using Algorithm.Infrastructure.EventArguments;
using Common.Extensions;
using Common.ValueRanges;
using ConsoleVersion.Attributes;
using ConsoleVersion.Enums;
using ConsoleVersion.EventArguments;
using ConsoleVersion.Extensions;
using ConsoleVersion.Interface;
using ConsoleVersion.Messages;
using ConsoleVersion.Model;
using GalaSoft.MvvmLight.Messaging;
using GraphLib.Base;
using GraphLib.Extensions;
using GraphLib.Interfaces;
using GraphViewModel;
using GraphViewModel.Interfaces;
using Interruptable.EventArguments;
using Interruptable.EventHandlers;
using Interruptable.Interface;
using Logging.Interface;
using NullObject.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleVersion.ViewModel
{
    internal sealed class PathFindingViewModel : PathFindingModel,
        IModel, IInterruptable, IRequireInt32Input, IRequireAnswerInput
    {
        public event ProcessEventHandler Interrupted;

        public string AlgorithmKeyInputMessage { private get; set; }

        public IValueInput<int> Int32Input { get; set; }
        public IValueInput<Answer> AnswerInput { get; set; }

        public PathFindingViewModel(ILog log, IGraph graph,
            BaseEndPoints endPoints, IEnumerable<IAlgorithmFactory> algorithmFactories)
            : base(log, graph, endPoints, algorithmFactories)
        {
            algorithmKeysValueRange = new InclusiveValueRange<int>(Algorithms.Length, 1);
            ConsoleKeystrokesHook.Instance.KeyPressed += OnConsoleKeyPressed;
            DelayTime = Constants.AlgorithmDelayTimeValueRange.LowerValueOfRange;
        }

        [MenuItem(MenuItemsNames.FindPath, MenuItemPriority.Highest)]
        public override void FindPath()
        {
            if (!endPoints.HasIsolators() && !Algorithm.IsNull())
            {
                try
                {
                    Console.CursorVisible = false;
                    base.FindPath();
                    ConsoleKeystrokesHook.Instance.StartHookingConsoleKeystrokes();
                    Console.CursorVisible = true;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
            else
            {
                log.Warn(MessagesTexts.EndPointsFirstlyMsg);
            }
        }

        [MenuItem(MenuItemsNames.ChooseAlgorithm, MenuItemPriority.High)]
        public void ChooseAlgorithm()
        {
            int algorithmKeyIndex = Int32Input.InputValue(AlgorithmKeyInputMessage, algorithmKeysValueRange) - 1;
            Algorithm = Algorithms[algorithmKeyIndex].Item2;
        }

        [MenuItem(MenuItemsNames.InputDelayTime, MenuItemPriority.Normal)]
        public void InputDelayTime()
        {
            if (IsVisualizationRequired)
            {
                DelayTime = Int32Input.InputValue(MessagesTexts.DelayTimeInputMsg, Constants.AlgorithmDelayTimeValueRange);
            }
        }

        [MenuItem(MenuItemsNames.Exit, MenuItemPriority.Lowest)]
        public void Interrupt()
        {
            ClearGraph();
            Interrupted?.Invoke(this, new ProcessEventArgs());
            Interrupted = null;
            ConsoleKeystrokesHook.Instance.KeyPressed -= OnConsoleKeyPressed;
        }

        [MenuItem(MenuItemsNames.ChooseEndPoints, MenuItemPriority.High)]
        public void ChooseExtremeVertex()
        {
            if (HasAnyVerticesToChooseAsEndPoints)
            {
                var selection = new EndPointsSelection(endPoints, graph, NumberOfAvailableIntermediate) { Int32Input = Int32Input };
                selection.ChooseEndPoints();
            }
            else
            {
                log.Warn(MessagesTexts.NoVerticesAsEndPointsMsg);
            }
        }

        [MenuItem(MenuItemsNames.ClearGraph, MenuItemPriority.Low)]
        public void ClearGraph()
        {
            Messenger.Default.Send(new ClearGraphMessage(), MessageTokens.MainModel);
        }

        [MenuItem(MenuItemsNames.ClearColors, MenuItemPriority.Low)]
        public void ClearColors()
        {
            Messenger.Default.Send(new ClearColorsMessage(), MessageTokens.MainModel);
        }

        [MenuItem(MenuItemsNames.ApplyVisualization, MenuItemPriority.Low)]
        public void ApplyVisualization()
        {
            var answer = AnswerInput.InputValue(MessagesTexts.ApplyVisualizationMsg, Constants.AnswerValueRange);
            IsVisualizationRequired = answer == Answer.Yes;
        }

        protected override void SummarizePathfindingResults()
        {
            string statistics = !path.IsNull() ? GetStatistics() : MessagesTexts.CouldntFindPathMsg;
            var message = new UpdateStatisticsMessage(statistics);
            Messenger.Default.Send(message, MessageTokens.MainView);
            visitedVerticesCount = 0;
        }

        protected override void OnVertexVisited(object sender, AlgorithmEventArgs e)
        {
            Stopwatch.StartNew().Pause(DelayTime).Cancel();
            base.OnVertexVisited(sender, e);
            var message = new UpdateStatisticsMessage(GetStatistics());
            Messenger.Default.Send(message, MessageTokens.MainView);
        }

        protected override void SubscribeOnAlgorithmEvents(PathfindingAlgorithm algorithm)
        {
            base.SubscribeOnAlgorithmEvents(algorithm);
            algorithm.Finished += ConsoleKeystrokesHook.Instance.CancelHookingConsoleKeystrokes;
        }

        private string GetStatistics()
        {
            string timerInfo = timer.ToString();
            string description = Algorithms.FirstOrDefault(item => item.Item2 == Algorithm).Item1;
            var pathfindingInfos = new object[] { path.PathLength, path.PathCost, visitedVerticesCount };
            string pathfindingInfo = string.Format(MessagesTexts.PathfindingStatisticsFormat, pathfindingInfos);
            return string.Join("\t", description, timerInfo, pathfindingInfo);
        }

        private void OnConsoleKeyPressed(object sender, ConsoleKeyPressedEventArgs e)
        {
            switch (e.PressedKey)
            {
                case ConsoleKey.Escape:
                    algorithm.Interrupt();
                    break;
                case ConsoleKey.UpArrow:
                    DelayTime = Constants.AlgorithmDelayTimeValueRange.ReturnInRange(DelayTime - 1);
                    break;
                case ConsoleKey.DownArrow:
                    DelayTime = Constants.AlgorithmDelayTimeValueRange.ReturnInRange(DelayTime + 1);
                    break;
            }
        }

        private int NumberOfAvailableIntermediate => graph.Size - graph.GetIsolatedCount() - 2;
        private bool HasAnyVerticesToChooseAsEndPoints => NumberOfAvailableIntermediate >= 0;

        private readonly InclusiveValueRange<int> algorithmKeysValueRange;
    }
}