﻿using Pathfinding.App.Console.DAL.Interface;
using Pathfinding.App.Console.DAL.Models.TransferObjects.Serialization;
using Pathfinding.App.Console.Interface;
using Pathfinding.App.Console.Localization;
using Pathfinding.App.Console.MenuItems.MenuItemPriority;
using Pathfinding.App.Console.Model;
using Pathfinding.GraphLib.Serialization.Core.Interface;
using Pathfinding.Logging.Interface;
using System.Collections.Generic;

namespace Pathfinding.App.Console.MenuItems.GraphSharingMenuItems.Export
{
    [LowPriority]
    internal sealed class SendGraphHistoryToNetworkMenuItem(IInput<(string Host, int Port)> input,
        IInput<int> intInput,
        ISerializer<IEnumerable<PathfindingHistorySerializationDto>> serializer,
        ILog log,
        IService<Vertex> service) : ExportGraphToNetworkMenuItem<PathfindingHistorySerializationDto>(input, intInput, serializer, log, service)
    {
        public override string ToString() => Languages.SendGraphHistory;

        protected override PathfindingHistorySerializationDto GetForSave(int graphId)
        {
            return service.GetSerializationHistory(graphId);
        }
    }
}
