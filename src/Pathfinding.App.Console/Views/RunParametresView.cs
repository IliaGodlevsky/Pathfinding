using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunParametresView : FrameView
{
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
        }
    }
}
