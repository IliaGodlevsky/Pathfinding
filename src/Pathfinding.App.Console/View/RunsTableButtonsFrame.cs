using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed partial class RunsTableButtonsFrame : FrameView
    {
        public RunsTableButtonsFrame([KeyFilter(KeyFilters.RunButtonsFrame)] Terminal.Gui.View[] children)
        {
            Initialize();
            Add(children);
        }
    }
}
