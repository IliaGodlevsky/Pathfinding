using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsPanel : FrameView
{
    public RunsPanel([KeyFilter(KeyFilters.RunsPanel)] View[] children)
    {
        Initialize();
        Add(children);
    }
}
