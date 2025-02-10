using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed partial class RunParametresView : FrameView
    {
        public RunParametresView([KeyFilter(KeyFilters.RunParametresView)]
            Terminal.Gui.View[] children)
        {
            X = Pos.Percent(25);
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Percent(90);
            Border = new();
            Add(children);
            foreach (var child in children)
            {
                this.Events().VisibleChanged
                    .Where(x => !Visible)
                    .Select(x => Visible)
                    .BindTo(child, x => x.Visible);
            }
        }
    }
}
