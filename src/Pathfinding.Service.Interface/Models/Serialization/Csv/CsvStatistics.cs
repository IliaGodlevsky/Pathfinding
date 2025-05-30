﻿using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Service.Interface.Models.Serialization.Csv;

internal class CsvStatistics
{
    public int GraphId { get; set; }

    public Algorithms Algorithm { get; set; }

    public Heuristics? Heuristics { get; set; } = null;

    public double? Weight { get; set; } = null;

    public StepRules? StepRule { get; set; } = null;

    public RunStatuses ResultStatus { get; set; }

    public double Elapsed { get; set; }

    public int Steps { get; set; }

    public double Cost { get; set; }

    public int Visited { get; set; }
}