using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal partial class RunsListView
    {
        private readonly ListView runList = new();

        private void Initialize()
        {
            runList.X = 0;
            runList.Y = 0;
            runList.Width = Dim.Fill();
            runList.Height = Dim.Fill();
            X = 0;
            Y = 1;
            Width = Dim.Percent(25);
            Height = Dim.Percent(80);
            Border = new Border()
            {
                BorderStyle = BorderStyle.Rounded,
                Title = Resource.Algorithms,
                BorderThickness = new Thickness(0)
            };
            Add(runList);
        }
    }
}
