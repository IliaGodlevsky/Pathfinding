using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms.Events
{
    public class SubPathFoundEventArgs(IGraphPath subPath) : EventArgs
    {
        public IGraphPath SubPath { get; } = subPath;
    }
}
