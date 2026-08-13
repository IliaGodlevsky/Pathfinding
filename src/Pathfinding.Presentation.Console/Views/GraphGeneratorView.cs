using NStack;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class GraphGeneratorView : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public GraphGeneratorView(IRequireGraphGeneratorViewModel viewModel)
    {
        X = Pos.Percent(40);
        Y = 4 + Pos.Percent(35);
        Width = Dim.Percent(33);
        Height = Dim.Fill(3);
        Border = new Border
        {
            BorderStyle = BorderStyle.Rounded,
            Padding = new Thickness(0),
            Title = "Generator"
        };

        var generators = viewModel.AllowedGenerators.ToArray();
        var options = new RadioGroup
        {
            X = 1,
            Y = 0,
            RadioLabels = [.. generators
                .Select(generator => ustring.Make(generator.ToStringRepresentation()))]
        };
        options.Events().SelectedItemChanged
            .Where(args => args.SelectedItem >= 0 && args.SelectedItem < generators.Length)
            .Select(args => generators[args.SelectedItem])
            .BindTo(viewModel, model => model.Generator)
            .DisposeWith(disposables);
        Add(options);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
