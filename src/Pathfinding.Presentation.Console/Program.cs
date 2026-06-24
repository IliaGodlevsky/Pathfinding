using Pathfinding.Presentation.Console;

await using var app = App
    .InitiateApp()
    .AddSqlite()
    .AddLogging()
    .AddGraphLayers()
    .AddDataTransfering()
    .AddPathfindingAlgorithms()
    .AddTransitPathfindingRangeCommands()
    .BuildApp();

app.RunApp();