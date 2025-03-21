namespace Pathfinding.Service.Interface;

public interface IStepRule
{
    double CalculateStepCost(
        IPathfindingVertex neighbour,
        IPathfindingVertex current);
}