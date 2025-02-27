using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsTableButtonsFrame : FrameView
    {
        public RunsTableButtonsFrame([KeyFilter(KeyFilters.RunButtonsFrame)] View[] children)
        {
            Initialize();
            Add(children);
        }
    }
}
