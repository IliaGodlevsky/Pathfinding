﻿using GraphLib.Base;
using GraphLib.Infrastructure;
using GraphLib.Interfaces;
using GraphLib.Interfaces.Factories;
using System;
using System.Windows.Input;

namespace WPFVersion.Model
{
    internal sealed class VertexEventHolder : BaseVertexEventHolder, IVertexEventHolder, INotifyCostChanged
    {
        public event CostChangedEventHandler CostChanged;

        public VertexEventHolder(IVertexCostFactory costFactory) : base(costFactory)
        {

        }

        protected override int GetWheelDelta(EventArgs e)
        {
            return e is MouseWheelEventArgs args ? args.Delta > 0 ? 1 : -1 : default;
        }

        public override void ChangeVertexCost(object sender, EventArgs e)
        {
            base.ChangeVertexCost(sender, e);
            if (sender is IVertex vertex)
            {
                var args = new CostChangedEventArgs(vertex.Cost, vertex);
                CostChanged?.Invoke(this, args);
            }
        }

        protected override void SubscribeToEvents(IVertex vertex)
        {
            if (vertex is Vertex vert)
            {
                vert.MouseRightButtonDown += Reverse;
                vert.MouseWheel += ChangeVertexCost;
            }
        }

        protected override void UnsubscribeFromEvents(IVertex vertex)
        {
            if (vertex is Vertex vert)
            {
                vert.MouseRightButtonDown -= Reverse;
                vert.MouseWheel -= ChangeVertexCost;
            }
        }
    }
}
