using Pathfinding.App.Console.Model;
using Pathfinding.Domain.Interface;

namespace Pathfinding.App.Console.ViewModel.Interface
{
    internal interface IRunFieldViewModel
    {
        RunModel SelectedRun { get; set; }

        IGraph<RunVertexModel> RunGraph { get; }
    }
}