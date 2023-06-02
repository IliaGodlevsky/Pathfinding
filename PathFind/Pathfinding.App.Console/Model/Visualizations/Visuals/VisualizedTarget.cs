﻿using Pathfinding.App.Console.Settings;

namespace Pathfinding.App.Console.Model.Visualizations.Visuals
{
    internal sealed class VisualizedTarget : VisualizedVertices
    {
        protected override string SettingsKey { get; } = nameof(Colours.TargetColor);
    }
}
