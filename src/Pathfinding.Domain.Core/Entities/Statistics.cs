using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Domain.Core.Entities
{
    public class Statistics : IEntity<int>
    {
        public int Id { get; set; }

        public int GraphId { get; set; }

        public Algorithms Algorithm { get; set; }

        public HeuristicFunctions? Heuristics { get; set; } = null;

        public double? Weight { get; set; } = null;

        public StepRules? StepRule { get; set; } = null;

        public RunStatuses ResultStatus { get; set; }

        public double Elapsed { get; set; }

        public int Steps { get; set; }

        public double Cost { get; set; }

        public int Visited { get; set; }
    }
}
