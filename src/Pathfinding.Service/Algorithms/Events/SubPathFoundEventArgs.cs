using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Algorithms.Events;

public class SubPathFoundEventArgs(IGraphPath subPath) : EventArgs
{
    public IGraphPath SubPath { get; } = subPath;
}
