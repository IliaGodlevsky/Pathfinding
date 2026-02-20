using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Service.Interface.Models.Undefined;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class RunsUpdatedMessage(List<RunStatisticsModel> updated)
    : ValueChangedMessage<List<RunStatisticsModel>>(updated);
