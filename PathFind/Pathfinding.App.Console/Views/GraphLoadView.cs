﻿using Pathfinding.App.Console.ViewModel;
using Pathfinding.Logging.Interface;

namespace Pathfinding.App.Console.Views
{
    internal sealed class GraphLoadView : View
    {
        public GraphLoadView(GraphLoadViewModel model, ILog log) : base(model, log)
        {
        }
    }
}
