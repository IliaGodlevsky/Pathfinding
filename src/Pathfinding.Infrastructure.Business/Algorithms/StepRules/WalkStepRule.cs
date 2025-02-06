using Pathfinding.Service.Interface;
using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.StepRules
{
    public sealed class WalkStepRule(IStepRule stepRule, int walkStepCost = 1) : IStepRule
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalculateStepCost(IPathfindingVertex neighbour, IPathfindingVertex current)
        {
            return stepRule.CalculateStepCost(neighbour, current) + walkStepCost;
        }
    }
}