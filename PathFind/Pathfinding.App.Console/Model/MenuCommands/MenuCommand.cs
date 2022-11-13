﻿using Pathfinding.App.Console.Delegates;
using Pathfinding.App.Console.Interface;
using Shared.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.App.Console.Model.MenuCommands
{
    internal sealed class MenuCommand : IMenuCommand
    {
        private readonly Command command;
        private readonly string header;
        private readonly IReadOnlyCollection<ConditionPair> conditions;

        public MenuCommand(string header, Command command, 
            IReadOnlyCollection<ConditionPair> conditionCommands)
        {
            this.header = header;
            this.command = command;
            conditions = conditionCommands;
        }

        public void Execute()
        {
            if (TryGetFailCondition(out var condition))
            {
                throw new ConditionFailedException(condition.FailMessage);
            }
            command();
        }

        private bool TryGetFailCondition(out ConditionPair faidCondition)
        {
            faidCondition = conditions.FirstOrDefault(condition => !condition.IsValidCondition());
            return faidCondition != null;
        }

        public override string ToString()
        {
            return header;
        }
    }
}
