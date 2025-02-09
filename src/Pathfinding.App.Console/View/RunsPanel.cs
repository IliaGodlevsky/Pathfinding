using Autofac.Features.AttributeFilters;
using DynamicData.Kernel;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed partial class RunsPanel : FrameView
    {
        public RunsPanel([KeyFilter(KeyFilters.RunsPanel)] Terminal.Gui.View[] children)
        {
            Initialize();
            Add(children);
        }
    }
}
