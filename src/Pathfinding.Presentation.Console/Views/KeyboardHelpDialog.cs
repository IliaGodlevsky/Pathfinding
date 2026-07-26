using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class KeyboardHelpDialog : Dialog
{
    private const int KeyColumnWidth = 16;

    private readonly Button closeButton = new("Close");

    public KeyboardHelpDialog()
    {
        Title = "Keyboard shortcuts";
        Width = 58;
        Height = 23;

        AddSection(1, "Navigation");
        AddShortcut(2, "Tab / Shift+Tab", "Move focus forward / backward");
        AddShortcut(3, "Arrow keys", "Move through tables and fields");
        AddShortcut(4, "Enter", "Activate the selected graph");

        AddSection(6, "Tables");
        AddShortcut(7, "Ctrl+A", "Select all rows in the focused table");
        AddShortcut(8, "Ctrl+R", "Restore the default run order");
        AddShortcut(9, "Mouse click", "Sort runs by the clicked column");

        AddSection(11, "Playback");
        AddShortcut(12, "Space", "Play / pause the selected run");
        AddShortcut(13, "Mouse wheel", "Move the run progress one step");
        AddShortcut(14, "Ctrl+wheel", "Move the run progress three steps");

        AddSection(16, "Help");
        AddShortcut(17, "F1", "Show keyboard shortcuts");
        AddShortcut(18, "F2", "Show the graph color legend");

        closeButton.Clicked += OnClose;
        AddButton(closeButton);
    }

    private void AddSection(int row, string title)
    {
        Add(new Label(title)
        {
            X = 2,
            Y = row,
            Width = Dim.Fill(2)
        });
    }

    private void AddShortcut(int row, string shortcut, string description)
    {
        Add(
            new Label(shortcut)
            {
                X = 4,
                Y = row,
                Width = KeyColumnWidth
            },
            new Label(description)
            {
                X = 4 + KeyColumnWidth,
                Y = row,
                Width = Dim.Fill(2)
            });
    }

    private static void OnClose()
    {
        Application.RequestStop();
    }

    protected override void Dispose(bool disposing)
    {
        closeButton.Clicked -= OnClose;
        base.Dispose(disposing);
    }
}
