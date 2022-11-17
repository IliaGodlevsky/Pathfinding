﻿using Pathfinding.App.Console;
using System;
using System.Text;

using ColorfulConsole = Colorful.Console;

namespace Pathfinding.App.Console.Model.FramedAxes
{
    internal abstract class FramedAbscissa : FramedAxis
    {
        private const string CoordinateDelimiter = "+";
        private const char FrameComponent = '-';

        protected const char Endl = '\n';
        protected const string NewLine = "\n";

        private static readonly string LargeSpace = new string(Space, Screen.WidthOfOrdinateView);

        private readonly int graphWidth;

        protected FramedAbscissa(int graphWidth) : base()
        {
            this.graphWidth = graphWidth;
        }

        public override void Display()
        {
            ColorfulConsole.SetCursorPosition(0, Screen.HeightOfGraphParametresView);
            ColorfulConsole.Write(GetFramedAxis());
        }

        protected string GetAbscissa()
        {
            var stringBuilder = new StringBuilder(LargeSpace);
            for (int i = 0; i < graphWidth; i++)
            {
                string line = GetAbscissaFragment(i);
                stringBuilder.Append(line);
            }
            return stringBuilder.Append(LargeSpace).ToString();
        }

        protected string GetHorizontalFrame()
        {
            var stringBuilder = new StringBuilder(LargeSpace);
            string fragment = GetHorizontalFrameFragment();
            int times = graphWidth;
            while (times-- > 0)
            {
                stringBuilder.Append(fragment);
            }
            return stringBuilder.Append(CoordinateDelimiter).ToString();
        }

        private string GetAbscissaFragment(int index)
        {
            return string.Concat(index, GetSpace(index));
        }

        private string GetHorizontalFrameFragment()
        {
            var frameComponent = new string(FrameComponent, LateralDistance - 1);
            return string.Concat(CoordinateDelimiter, frameComponent);
        }

        private string GetSpace(int index)
        {
            int indexLog = index.ToString().Length;
            int count = Math.Abs(LateralDistance - indexLog);
            return new string(Space, count);
        }
    }
}
