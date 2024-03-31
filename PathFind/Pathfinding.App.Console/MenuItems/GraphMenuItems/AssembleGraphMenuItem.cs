﻿using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.DAL.Interface;
using Pathfinding.App.Console.Interface;
using Pathfinding.App.Console.Localization;
using Pathfinding.App.Console.MenuItems.MenuItemPriority;
using Pathfinding.App.Console.Model;
using Pathfinding.GraphLib.Factory.Interface;
using Shared.Random;

namespace Pathfinding.App.Console.MenuItems.GraphMenuItems
{
    [HighestPriority]
    internal sealed class AssembleGraphMenuItem(IMessenger messenger,
        IGraphAssemble<Vertex> assemble,
        IRandom random,
        IService<Vertex> service,
        IInput<string> input)
        : GraphCreatingMenuItem(messenger, assemble, random, service, input)
    {
        public override string ToString()
        {
            return Languages.AssembleGraph;
        }
    }
}
