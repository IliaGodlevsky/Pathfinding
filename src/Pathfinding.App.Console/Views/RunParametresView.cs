using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunParametresView : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public RunParametresView(View[] children)
    {
        X = Pos.Percent(50);
        Width = Dim.Percent(50);
        Height = Dim.Fill();
        Border = new();
        Add(children);
        for (int i = 0; i < children.Length; i++)
        {
            children[i].Y = i == 0 ? 1 : Pos.Bottom(children[i - 1]);
            //this.Events().VisibleChanged
            //    .Where(x => !Visible)
            //    .Select(x => Visible)
            //    .BindTo(children[i], x => x.Visible)
            //    .DisposeWith(disposables);
        }
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
