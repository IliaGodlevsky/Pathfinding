﻿using GalaSoft.MvvmLight.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Interface;
using Pathfinding.App.Console.Localization;
using Pathfinding.App.Console.MenuItems.MenuItemPriority;

namespace Pathfinding.App.Console.MenuItems.GraphMenuItems
{
    [HighPriority]
    internal sealed class EnterGraphParametresMenuItem : GraphMenuItem
    {
        public EnterGraphParametresMenuItem(IMessenger messenger, IInput<int> input)
            : base(messenger, input)
        {

        }

        public override void Execute()
        {
            using (Cursor.UseCurrentPositionWithClean())
            {
                int width = input.Input(Languages.GraphWidthInputMsg, Constants.GraphWidthValueRange);
                int length = input.Input(Languages.GraphHeightInputMsg, Constants.GraphLengthValueRange);
                messenger.SendData((Width: width, Length: length), Tokens.Graph);
            }
        }

        public override string ToString()
        {
            return Languages.InputGraphParametres;
        }
    }
}
