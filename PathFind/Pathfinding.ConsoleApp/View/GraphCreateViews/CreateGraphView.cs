﻿using Pathfinding.ConsoleApp.ViewModel;
using System.Linq;
using Terminal.Gui;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Autofac.Features.AttributeFilters;
using Pathfinding.ConsoleApp.Injection;
using System.Reactive;
using Pathfinding.ConsoleApp.Messages.View;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.ConsoleApp.View.GraphCreateViews
{
    internal sealed partial class CreateGraphView : FrameView
    {
        private readonly CreateGraphViewModel viewModel;
        private readonly IMessenger messenger;
        private readonly CompositeDisposable disposables = new();
        private readonly Terminal.Gui.View[] children;

        public CreateGraphView([KeyFilter(KeyFilters.CreateGraphView)]IEnumerable<Terminal.Gui.View> children,
            CreateGraphViewModel viewModel,
            [KeyFilter(KeyFilters.Views)]IMessenger messenger)
        {
            this.viewModel = viewModel;
            this.messenger = messenger;
            Initialize();
            this.children = children.ToArray();
            Add(this.children);
            var hideWindowCommand = ReactiveCommand.Create<MouseEventArgs, Unit>(Hide,
                this.viewModel.CreateCommand.CanExecute);
            var commands = new[] { hideWindowCommand, this.viewModel.CreateCommand };
            var combined = ReactiveCommand.CreateCombined(commands);
            createButton.Events()
                .MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .InvokeCommand(combined)
                .DisposeWith(disposables);

            cancelButton.MouseClick += OnCancelClicked;
            messenger.Register<OpenGraphCreateViewRequest>(this, OnOpenCreateGraphViewRequestRecieved);
        }

        private void OnOpenCreateGraphViewRequestRecieved(object recipient,
            OpenGraphCreateViewRequest request)
        {
            Visible = true;
            children.ForEach(x => x.Visible = true);
        }

        private void OnCancelClicked(MouseEventArgs e)
        {
            if (e.MouseEvent.Flags == MouseFlags.Button1Clicked)
            {
                Hide(e);
                Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);
            }
        }

        private Unit Hide(MouseEventArgs e)
        {
            Visible = false;
            children.ForEach(x => x.Visible = false);
            return Unit.Default;
        }
    }
}
