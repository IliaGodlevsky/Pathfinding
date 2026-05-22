using Autofac.Features.AttributeFilters;
using Pathfinding.Presentation.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunsTableButtonsFrame : FrameView
{
    public RunsTableButtonsFrame([KeyFilter(KeyFilters.RunButtonsFrame)] View[] children)
    {
        Initialize();
        Add(children);
    }
}
