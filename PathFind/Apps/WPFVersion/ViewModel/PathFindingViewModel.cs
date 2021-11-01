﻿using Algorithm.Base;
using Algorithm.Factory;
using Algorithm.Infrastructure.EventArguments;
using Common.Extensions;
using Common.Interface;
using GalaSoft.MvvmLight.Messaging;
using GraphLib.Base;
using GraphLib.Interfaces;
using GraphViewModel;
using GraphViewModel.Interfaces;
using Interruptable.EventArguments;
using Logging.Interface;
using NullObject.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using WPFVersion.Enums;
using WPFVersion.Extensions;
using WPFVersion.Infrastructure;
using WPFVersion.Messages;

namespace WPFVersion.ViewModel
{
    internal class PathFindingViewModel : PathFindingModel, IModel, IViewModel
    {
        public event EventHandler WindowClosed;

        public ICommand ConfirmPathFindAlgorithmChoice { get; }
        public ICommand CancelPathFindAlgorithmChoice { get; }

        public PathFindingViewModel(ILog log, IGraph graph,
            BaseEndPoints endPoints, IEnumerable<IAlgorithmFactory> algorithmFactories)
            : base(log, graph, endPoints, algorithmFactories)
        {
            ConfirmPathFindAlgorithmChoice = new RelayCommand(ExecuteConfirmPathFindAlgorithmChoice,
                CanExecuteConfirmPathFindAlgorithmChoice);
            CancelPathFindAlgorithmChoice = new RelayCommand(ExecuteCloseWindowCommand);
            Messenger.Default.Register<AlgorithmIndexMessage>(this, MessageTokens.PathfindingModel, SetAlgorithmIndex);
            Messenger.Default.Register<DelayTimeChangedMessage>(this, MessageTokens.PathfindingModel, SetAlgorithmDelayTime);
            DelayTime = Convert.ToInt32(Constants.AlgorithmDelayTimeValueRange.LowerValueOfRange);
        }

        private void SetAlgorithmIndex(AlgorithmIndexMessage message)
        {
            Messenger.Default.Unregister<AlgorithmIndexMessage>(this, MessageTokens.PathfindingModel, SetAlgorithmIndex);
            Index = message.Index;
        }

        private void SetAlgorithmDelayTime(DelayTimeChangedMessage message)
        {
            if (Index == message.Index)
            {
                DelayTime = message.DelayTime;
            }
        }

        protected override void OnAlgorithmStarted(object sender, ProcessEventArgs e)
        {
            base.OnAlgorithmStarted(sender, e);
            string algorithmName = Algorithm.GetDescriptionAttributeValueOrTypeName();
            var message = new AlgorithmStartedMessage(algorithm, algorithmName, DelayTime);
            Messenger.Default.Send(message, MessageTokens.AlgorithmStatisticsModel);
        }

        protected override void SummarizePathfindingResults()
        {
            var status = !path.IsNull() ? AlgorithmStatus.Finished : AlgorithmStatus.Failed;
            string time = timer.ToFormattedString();
            var message = new UpdateStatisticsMessage(Index,
                time, visitedVerticesCount, path.PathLength, path.PathCost);
            var statusMessage = new AlgorithmStatusMessage(status, Index);
            Messenger.Default.SendMany(MessageTokens.AlgorithmStatisticsModel, message, statusMessage);
            var pathFoundMessage = new PathFoundMessage(algorithm, path);
            Messenger.Default.Send(pathFoundMessage, MessageTokens.VisualizationModel);
        }

        protected override async void OnVertexVisited(object sender, AlgorithmEventArgs e)
        {
            Stopwatch.StartNew().Pause(DelayTime).Cancel();
            string time = timer.ToFormattedString();
            var message = new UpdateStatisticsMessage(Index, time, visitedVerticesCount);
            await Messenger.Default.SendAsync(message, MessageTokens.AlgorithmStatisticsModel);
            visitedVerticesCount++;
        }

        protected override void OnVertexEnqueued(object sender, AlgorithmEventArgs e)
        {

        }

        protected override void OnAlgorithmInterrupted(object sender, ProcessEventArgs e)
        {
            base.OnAlgorithmInterrupted(sender, e);
            var message = new AlgorithmStatusMessage(AlgorithmStatus.Interrupted, Index);
            Messenger.Default.Send(message, MessageTokens.AlgorithmStatisticsModel);
        }

        protected override void OnAlgorithmFinished(object sender, ProcessEventArgs e)
        {
            base.OnAlgorithmFinished(sender, e);
            var message = new AlgorithmStatusMessage(AlgorithmStatus.Finished, Index);
            Messenger.Default.Send(message, MessageTokens.AlgorithmStatisticsModel);
            Messenger.Default.Unregister<DelayTimeChangedMessage>(this, MessageTokens.PathfindingModel, SetAlgorithmDelayTime);
        }

        private void ExecuteCloseWindowCommand(object param)
        {
            WindowClosed?.Invoke(this, EventArgs.Empty);
            WindowClosed = null;
        }

        private void ExecuteConfirmPathFindAlgorithmChoice(object param)
        {
            ExecuteCloseWindowCommand(null);
            base.FindPath();
        }

        protected override void SubscribeOnAlgorithmEvents(PathfindingAlgorithm algorithm)
        {
            if (IsVisualizationRequired)
            {
                var message = new AlgorithmChosenMessage(algorithm, endPoints);
                Messenger.Default.Send(message, MessageTokens.VisualizationModel);
            }
            base.SubscribeOnAlgorithmEvents(algorithm);

        }

        private bool CanExecuteConfirmPathFindAlgorithmChoice(object param)
        {
            return Algorithms.Any(item => item.Item2 == Algorithm);
        }

        private int Index { get; set; }
    }
}