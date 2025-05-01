using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Service.Interface.Models.Undefined;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal sealed class RunsUpdatedMessage(RunStatisticsModel[] updated)
    : ValueChangedMessage<RunStatisticsModel[]>(updated);
