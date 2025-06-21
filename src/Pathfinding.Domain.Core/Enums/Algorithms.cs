namespace Pathfinding.Domain.Core.Enums;

// Do not change the values of the enums, they are
// used to identify the algorithms in the database
public enum Algorithms
{
    Dijkstra = 0,
    BidirectDijkstra = 1,
    AStar = 2,
    BidirectAStar = 3,
    Lee = 4,
    BidirectLee = 5,
    AStarLee = 6,
    DistanceFirst = 7,
    CostGreedy = 8,
    AStarGreedy = 9,
    DepthFirst = 10,
    Snake = 11,
    Random = 12,
    BidirectRandom = 13,
    DepthFirstRandom = 14,
    IdaStar = 15
}