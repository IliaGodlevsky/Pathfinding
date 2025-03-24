using Pathfinding.Service.Interface;
using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.StepRules;

public sealed class DefaultStepRule : IStepRule
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double CalculateStepCost(
        IPathfindingVertex neighbour,
        IPathfindingVertex current)
    {
        return neighbour.Cost.CurrentCost;
    }
}