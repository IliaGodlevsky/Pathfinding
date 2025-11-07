using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface;

public interface IRequestService<T> :
    IGraphRequestService<T>,
    IStatisticsRequestService,
    IGraphInfoRequestService,
    IRangeRequestService<T>
    where T : IVertex, IEntity<long>
{
}
