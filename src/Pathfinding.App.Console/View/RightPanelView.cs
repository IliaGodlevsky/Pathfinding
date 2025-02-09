using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed class RightPanelView : Window
    {
        public RightPanelView([KeyFilter(KeyFilters.RightPanel)] Terminal.Gui.View[] children)
        {
            X = Pos.Percent(66);
            Y = 0;
            Width = Dim.Percent(34);
            Height = Dim.Fill();
            Border = new Border();

            Add(children);
        }
    }
}
