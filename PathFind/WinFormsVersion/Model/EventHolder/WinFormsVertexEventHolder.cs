﻿using System;
using System.Windows.Forms;
using WinFormsVersion.Vertex;
using GraphLibrary.EventHolder;
using GraphLibrary.Vertex.Interface;
using GraphLibrary.Vertex;

namespace WinFormsVersion.EventHolder
{
    internal class WinFormsVertexEventHolder : VertexEventHolder
    {
        protected override int GetWheelDelta(EventArgs e)
        {
            return (e as MouseEventArgs).Delta;
        }

        public override void ReversePolarity(object sender, EventArgs e)
        {
            if ((e as MouseEventArgs).Button == MouseButtons.Right)
                base.ReversePolarity(sender, e);            
        }

        protected override void ChargeVertex(IVertex vertex)
        {
            if (vertex == null)
                return;
            (vertex as WinFormsVertex).MouseClick += ChooseExtremeVertices;
            (vertex as WinFormsVertex).MouseClick += ReversePolarity;
            (vertex as WinFormsVertex).MouseWheel += ChangeVertexValue;
        }

        public override void ChooseExtremeVertices(object sender, EventArgs e)
        {
            if ((e as MouseEventArgs).Button == MouseButtons.Left)           
                base.ChooseExtremeVertices(sender, e);           
        }
    }
}
