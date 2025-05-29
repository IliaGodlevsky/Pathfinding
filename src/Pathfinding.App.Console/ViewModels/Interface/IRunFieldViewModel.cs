using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Interface;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRunFieldViewModel
{
    RunModel SelectedRun { get; set; }

    IGraph<RunVertexModel> RunGraph { get; }
}