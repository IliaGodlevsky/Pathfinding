namespace Pathfinding.Service.Interface;

public interface IHeuristic
{
    double Calculate(IPathfindingVertex first, IPathfindingVertex second);
}