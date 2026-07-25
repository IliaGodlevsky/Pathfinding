using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class KeyboardHelpDialog : Dialog
{
    private readonly Button closeButton = new("Close");

    public KeyboardHelpDialog()
    {
        Title = "Keyboard shortcuts";
        Width = 62;
        Height = 17;

        AddShortcut(1, "F1", "Open keyboard help");
        AddShortcut(2, "F2", "Open the graph color legend");
        AddShortcut(4, "Tab / Shift+Tab", "Move focus forward / backward");
        AddShortcut(5, "Arrow keys", "Move through tables and fields");
        AddShortcut(6, "Enter", "Activate the selected graph");
        AddShortcut(8, "Ctrl+A", "Select all rows in the focused table");
        AddShortcut(9, "Ctrl+R", "Restore the default run order");

        Add(new Label("Mouse: click a run-table column heading to sort it.")
        {
            X = 2,
            Y = 11,
            Width = Dim.Fill(2)
        });

        closeButton.Clicked += OnClose;
        AddButton(closeButton);
    }

    private void AddShortcut(int row, string shortcut, string description)
    {
        Add(
            new Label(shortcut)
            {
                X = 2,
                Y = row,
                Width = 18
            },
            new Label(description)
            {
                X = 22,
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
