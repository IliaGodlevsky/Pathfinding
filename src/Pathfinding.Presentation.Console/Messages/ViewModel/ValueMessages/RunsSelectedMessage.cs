using CommunityToolkit.Mvvm.Messaging.Messages;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;

internal sealed class RunsSelectedMessage(RunInfoModel[] selectedRuns)
    : ValueChangedMessage<RunInfoModel[]>(selectedRuns);
