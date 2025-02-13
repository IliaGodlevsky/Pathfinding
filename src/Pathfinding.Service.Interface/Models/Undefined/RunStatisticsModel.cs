using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Service.Interface.Models.Undefined
{
    public record RunStatisticsModel : IAlgorithmBuildInfo
    {
        public int Id { get; set; }

        public int GraphId { get; set; }

        public Algorithms Algorithm { get; set; }

        public HeuristicFunctions? Heuristics { get; set; } = null;

        public double? Weight { get; set; } = null;

        public StepRules? StepRule { get; set; } = null;

        public int Visited { get; set; }

        public RunStatuses ResultStatus { get; set; }

        public TimeSpan Elapsed { get; set; }

        public int Steps { get; set; }

        public double Cost { get; set; }
    }
}