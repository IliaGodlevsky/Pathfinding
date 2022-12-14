﻿using Pathfinding.App.Console.Interface;
using System.Collections.Generic;

namespace Pathfinding.App.Console.ViewModel
{
    internal sealed class GraphUnit : Unit
    {
        public GraphUnit(IReadOnlyCollection<IMenuItem> menuItems)
            : base(menuItems)
        {

        }
    }
}