using Pathfinding.Domain.Interface;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRunFieldViewModel
{
    RunModel SelectedRun { get; set; }

    IGraph<RunVertexModel> RunGraph { get; }
}