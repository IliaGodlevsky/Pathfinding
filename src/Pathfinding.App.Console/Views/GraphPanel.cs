using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphPanel : FrameView
{
    public GraphPanel([KeyFilter(KeyFilters.GraphPanel)] View[] children)
    {
        Initialize();
        Add(children);
    }
}
