using Autofac;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Views;
using Terminal.Gui;

Application.Init();
await using var scope = Modules.Build();
using var main = scope.Resolve<MainView>();
Application.Top.Add(main);
Application.Run(_ => true);