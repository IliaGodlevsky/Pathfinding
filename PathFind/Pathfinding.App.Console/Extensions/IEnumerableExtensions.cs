﻿using Pathfinding.App.Console.Interface;
using Pathfinding.App.Console.Model;
using Pathfinding.App.Console.Views;
using Pathfinding.Logging.Interface;
using Shared.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.App.Console.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static IDisplayable CreateMenuList<T>(this IEnumerable<T> items, int columnsNumber = 2)
        {
            return new MenuList(items.Select(item => item.ToString()), columnsNumber);
        }

        public static void Display(this IEnumerable<IDisplayable> displayables)
        {
            displayables.ForEach(displayable => displayable.Display());
        }
    }
}
