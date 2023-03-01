﻿using GalaSoft.MvvmLight.Messaging;
using Pathfinding.App.Console.Interface;
using Pathfinding.App.Console.Messages;
using Pathfinding.VisualizationLib.Core.Interface;
using System;
using System.Collections.Generic;

namespace Pathfinding.App.Console.Model.Visualizations
{
    internal sealed class RangeVisualization : IRangeVisualization<Vertex>, ICanRecieveMessage
    {
        private readonly HashSet<Vertex> vertices = new();
        private readonly IMessenger messenger;

        public ConsoleColor SourceVertexColor { get; set; } = ConsoleColor.Magenta;

        public ConsoleColor TargetVertexColor { get; set; } = ConsoleColor.Red;

        public ConsoleColor TransitVertexColor { get; set; } = ConsoleColor.Green;

        public RangeVisualization(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public bool IsVisualizedAsRange(Vertex visualizable)
        {
            return vertices.Contains(visualizable);
        }

        public void VisualizeAsSource(Vertex visualizable)
        {
            vertices.Add(visualizable);
            visualizable.Color = SourceVertexColor;
        }

        public void VisualizeAsTarget(Vertex visualizable)
        {
            vertices.Add(visualizable);
            visualizable.Color = TargetVertexColor;
        }

        public void VisualizeAsTransit(Vertex visualizable)
        {
            vertices.Add(visualizable);
            visualizable.Color = TransitVertexColor;
        }

        public void RegisterHanlders(IMessenger messenger)
        {
            messenger.Register<AskForRangeColorsMessage>(this, AskForColors);
            messenger.Register<RangeColorsMessage>(this, MessageTokens.RangeColors, ColorsRecieved);
            messenger.Register<RemoveFromVisualizedMessage>(this, MessageTokens.RangeColors, RemoveVertex);
        }

        private void AskForColors(AskForRangeColorsMessage msg)
        {
            var message = new RangeColorsMessage(SourceVertexColor, TargetVertexColor, TransitVertexColor);
            messenger.Send(message, MessageTokens.RangeColorsChangeItem);
        }

        private void ColorsRecieved(RangeColorsMessage msg)
        {
            SourceVertexColor = msg.SourceColor;
            TransitVertexColor = msg.TransitColor;
            TargetVertexColor = msg.TargetColor;
        }

        private void RemoveVertex(RemoveFromVisualizedMessage msg)
        {
            vertices.Remove(msg.Vertex);
        }
    }
}
