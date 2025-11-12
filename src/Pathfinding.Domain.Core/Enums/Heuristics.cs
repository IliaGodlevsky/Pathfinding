namespace Pathfinding.Domain.Core.Enums;

// Do not change the values of the enums, they are
// used to identify the heuristics in the database
public enum Heuristics
{
    Euclidean,
    Chebyshev,
    Diagonal,
    Manhattan,
    Canberra,
    Hamming
}
