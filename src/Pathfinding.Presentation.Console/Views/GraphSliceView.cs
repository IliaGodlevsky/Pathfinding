using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Data.Extensions;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class GraphSliceView : FrameView
{
    private readonly Button nextSlice = new(">");
    private readonly Button prevSlice = new("<");
    private readonly Button firstSlice = new("<<");
    private readonly Button lastSlice = new(">>");
    private readonly Label totalSlices = new();
    private readonly TextField currentSlice = new();
    private readonly CompositeDisposable disposables = [];

    private readonly IMessenger messenger;

    private int ttlSlice = 0;
    private int TotalSlices 
    { 
        get => ttlSlice;
        set
        {
            ttlSlice = value;
            totalSlices.Text = ttlSlice.ToString();
        }
    }

    private int currentSlc = 0;
    private int CurrentSlice 
    { 
        get => currentSlc;
        set
        {
            currentSlc = SliceRange.ReturnInRange(value);
            currentSlice.Text = currentSlc.ToString();
            messenger.Send(new SliceChangedMessage(currentSlc));
        }
    }

    private InclusiveValueRange<int> SliceRange { get; set; } = (0, 0);

    public GraphSliceView([KeyFilter(KeyFilters.Views)] IMessenger messenger)
    {
        this.messenger = messenger;
        Border = new()
        {
            BorderBrush = Color.BrightYellow,
            BorderStyle = BorderStyle.Rounded,
            Title = "Depth"
        };
        X = 0;
        Y = 0;
        Width = Dim.Percent(66);
        Height = Dim.Percent(7);
        firstSlice.X = Pos.Percent(40);
        firstSlice.Y = 0;
        firstSlice.Width = 4;
        prevSlice.X = Pos.Right(firstSlice) + 1;
        prevSlice.Y = 0;
        prevSlice.Width = 3;
        currentSlice.X = Pos.Right(prevSlice) + 1;
        currentSlice.Y = 0;
        currentSlice.Width = 4;
        totalSlices.X = Pos.Right(currentSlice) + 1;
        totalSlices.Y = 0;
        totalSlices.Width = 4;
        nextSlice.X = Pos.Right(totalSlices) + 1;
        nextSlice.Y = 0;
        nextSlice.Width = 3;
        lastSlice.X = Pos.Right(nextSlice) + 1;
        lastSlice.Y = 0;
        lastSlice.Width = 4;
        Bind(nextSlice, () => CurrentSlice++);
        Bind(prevSlice, () => CurrentSlice--);
        Bind(firstSlice, () => CurrentSlice = 1);
        Bind(lastSlice, () => CurrentSlice = TotalSlices);
        CurrentSlice = 0;
        TotalSlices = 0;

        Add(nextSlice, prevSlice, 
            firstSlice, lastSlice, 
            totalSlices, currentSlice);
        messenger.RegisterHandler<GraphActivatedMessage>(this, OnGraphActivated);
    }

    private void Bind(View view, Func<int> function)
    {
        view.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(x => function())
            .DisposeWith(disposables);
    }

    private void OnGraphActivated(GraphActivatedMessage msg)
    {
        TotalSlices = Math.Max(1, msg.Graph.GetDepth());
        SliceRange = (1, TotalSlices);
        CurrentSlice = 1;
    }
}
