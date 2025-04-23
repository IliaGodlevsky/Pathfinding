using Pathfinding.Service.Interface.Models.Undefined;

namespace Pathfinding.App.Console.Messages.ViewModel;

internal sealed record RunCreatedMessaged(IReadOnlyCollection<RunStatisticsModel> Models);
