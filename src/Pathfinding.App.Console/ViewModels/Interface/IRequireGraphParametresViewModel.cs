namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRequireGraphParametresViewModel
    {
        public int Width { get; set; }

        public int Length { get; set; }

        public int Obstacles { get; set; }
    }
}
