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
        X = 1;
        Y = Pos.Percent(15);
        Width = Dim.Fill(3);
        Height = 2;
        Border = new Border { BorderStyle = BorderStyle.None };

        var generators = viewModel.AllowedGenerators.ToArray();
        var label = new Label("Generator")
        {
            X = 1,
            Y = 0
        };
        var options = new RadioGroup
        {
            X = Pos.Right(label) + 2,
            Y = 0,
            RadioLabels = generators
                .Select(generator => ustring.Make(generator.ToStringRepresentation()))
                .ToArray()
        };
        options.Events().SelectedItemChanged
            .Where(args => args.SelectedItem >= 0 && args.SelectedItem < generators.Length)
            .Select(args => generators[args.SelectedItem])
            .BindTo(viewModel, model => model.Generator)
            .DisposeWith(disposables);
        Add(label, options);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
