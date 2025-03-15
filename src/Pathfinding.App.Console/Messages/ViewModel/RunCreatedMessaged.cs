using Pathfinding.Service.Interface.Models.Undefined;

namespace Pathfinding.App.Console.Messages.ViewModel;

internal sealed record class RunCreatedMessaged(IReadOnlyCollection<RunStatisticsModel> Models);
