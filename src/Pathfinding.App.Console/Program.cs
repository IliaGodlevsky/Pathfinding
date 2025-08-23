using Autofac;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Views;
using Terminal.Gui;

Application.Init();
await using var container = Modules.Build();
using var main = container.Resolve<MainView>();
Application.Top.Add(main);
Application.Run(x => true);