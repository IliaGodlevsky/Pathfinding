using Autofac;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Views;
using ReactiveUI.Builder;
using Terminal.Gui;

RxAppBuilder.CreateReactiveUIBuilder().BuildApp();
Application.Init();
await using var container = Modules.Build();
using var main = container.Resolve<MainView>();
Application.Top.Add(main);
Application.Run(x => true);