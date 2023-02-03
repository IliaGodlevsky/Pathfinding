﻿using Autofac;
using GalaSoft.MvvmLight.Messaging;
using Pathfinding.App.Console.DependencyInjection;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Model;
using Pathfinding.GraphLib.Core.Realizations.Coordinates;
using Pathfinding.GraphLib.Core.Realizations.Graphs;
using Shared.Extensions;
using System;
using System.Drawing;

namespace Pathfinding.App.Console
{
    internal static class Screen
    {
        public const int HeightOfAbscissaView = 2;
        public const int HeightOfGraphParametresView = 1;

        public static readonly int WidthOfOrdinateView
            = (Constants.GraphLengthValueRange.UpperValueOfRange - 1).GetDigitsNumber() + 1;
        public static readonly int YCoordinatePadding = WidthOfOrdinateView - 1;

        private static readonly object recipient = new object();
        private static readonly IMessenger messenger;

        private static int CurrentMaxValueOfRange;

        private static Graph2D<Vertex> Graph { get; set; } = Graph2D<Vertex>.Empty;

        public static int LateralDistanceBetweenVertices { get; private set; }

        public static Point GraphFieldPosition { get; }

        public static Point StatisticsPosition { get; private set; } = Point.Empty;

        static Screen()
        {
            messenger = Registry.Container.Resolve<IMessenger>();
            messenger.Register<CostRangeChangedMessage>(recipient, OnCostRangeChanged);
            messenger.Register<PathfindingStatisticsMessage>(recipient, OnStatisticsUpdated);
            messenger.Register<GraphCreatedMessage>(recipient, MessageTokens.Screen, OnNewGraphCreated);
            int x = WidthOfOrdinateView;
            int y = HeightOfAbscissaView + HeightOfGraphParametresView;
            GraphFieldPosition = new(x, y);
        }

        public static void SetCursorPositionUnderMenu(int menuOffset = 0)
        {
            System.Console.SetCursorPosition(StatisticsPosition.X, StatisticsPosition.Y + menuOffset);
        }

        private static void OnNewGraphCreated(GraphCreatedMessage message)
        {
            Graph = message.Graph;
            int pathFindingStatisticsOffset = message.Graph.Length + HeightOfAbscissaView * 2 + HeightOfGraphParametresView;
            StatisticsPosition = new(0, pathFindingStatisticsOffset);
            RecalculateVerticesConsolePosition();
        }

        private static void OnCostRangeChanged(CostRangeChangedMessage message)
        {
            int upperValueRange = message.CostRange.UpperValueOfRange;
            int lowerValueRange = message.CostRange.LowerValueOfRange;
            int max = Math.Max(Math.Abs(upperValueRange), Math.Abs(lowerValueRange));
            CurrentMaxValueOfRange = max;
            RecalculateVerticesConsolePosition();
        }

        private static void RecalculateVerticesConsolePosition()
        {
            LateralDistanceBetweenVertices = CalculateLateralDistanceBetweenVertices();
            Graph.ForEach(RecalculateConsolePosition);
        }

        private static void OnStatisticsUpdated(PathfindingStatisticsMessage message)
        {
            Cursor.SetPosition(StatisticsPosition);
            System.Console.Write(message.Statistics.PadRight(System.Console.BufferWidth));
        }

        private static void RecalculateConsolePosition(Vertex vertex)
        {
            var point = (Coordinate2D)vertex.Position;
            int left = GraphFieldPosition.X + point.X * LateralDistanceBetweenVertices;
            int top = GraphFieldPosition.Y + point.Y;
            vertex.ConsolePosition = new(left, top);
        }

        private static int CalculateLateralDistanceBetweenVertices()
        {
            int costWidth = CurrentMaxValueOfRange.GetDigitsNumber();
            int width = (Constants.GraphWidthValueRange.UpperValueOfRange - 1).GetDigitsNumber();
            return costWidth >= width ? costWidth + 2 : width + width - costWidth;
        }
    }
}