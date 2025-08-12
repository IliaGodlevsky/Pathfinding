using Autofac;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Views;
using Pathfinding.Logging.Interface;
using Terminal.Gui;

Application.Init();
await using var context = Modules.Build();
using var main = context.Resolve<MainView>();
var log = context.Resolve<ILog>();
Application.Top.Add(main);
Application.Run(x =>
{
    log.Fatal(x, x.Message);
    return false;
});