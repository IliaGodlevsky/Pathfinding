namespace Pathfinding.Service.Interface.Requests.Read;

public record ReadStatisticsRequest(
    int GraphId, 
    int Take = int.MaxValue, 
    int Skip = 0);
