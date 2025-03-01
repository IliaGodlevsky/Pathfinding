using System.ComponentModel;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRequireGraphParametresViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;

        public int Width { get; set; }

        public int Length { get; set; }

        public int Obstacles { get; set; }
    }
}
