using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections;
using System.Diagnostics;

namespace Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;

[DebuggerDisplay("Null")]
public sealed class NullGraphPath : Singleton<NullGraphPath, IGraphPath>, IGraphPath
{
    public double Cost => 0;

    public int Count => 0;

    private NullGraphPath()
    {

    }

    public IEnumerator<Coordinate> GetEnumerator()
    {
        return Enumerable.Empty<Coordinate>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
