using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed class RightPanelView : View
    {
        public RightPanelView([KeyFilter(KeyFilters.RightPanel)] View[] children)
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
