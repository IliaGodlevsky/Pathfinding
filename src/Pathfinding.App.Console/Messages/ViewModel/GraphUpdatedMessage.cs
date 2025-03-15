using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Messages.ViewModel;

internal sealed record class GraphUpdatedMessage(GraphInformationModel Model);
