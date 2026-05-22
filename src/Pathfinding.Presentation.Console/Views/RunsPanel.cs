using Autofac.Features.AttributeFilters;
using Pathfinding.Presentation.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunsPanel : FrameView
{
    public RunsPanel([KeyFilter(KeyFilters.RunsPanel)] View[] children)
    {
        Initialize();
        Add(children);
    }
}
