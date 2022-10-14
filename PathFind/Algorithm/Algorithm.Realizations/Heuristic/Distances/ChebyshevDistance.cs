﻿using System;

namespace Algorithm.Realizations.Heuristic.Distances
{
    public sealed class ChebyshevDistance : DistanceFunction
    {
        protected override double AggregateMethod(double a, double b)
        {
            return Math.Max(a, b);
        }

        protected override double ZipMethod(int first, int second)
        {
            return Math.Abs(first - second);
        }
    }
}