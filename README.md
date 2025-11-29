# Pathfinding (Terminal GUI)

A .NET 8 pathfinding sandbox with a **Terminal.Gui** interface. Build and tweak grid-based graphs, run classic pathfinding algorithms side-by-side, and persist your experiments for later comparison. The terminal UI keeps everything keyboard-driven and scriptable while still feeling like a full application.

## Key features
- **Terminal GUI workspace**: run the console app to get a tiled TUI for editing graphs, configuring algorithms, and inspecting results.
- **Rich algorithm catalog**: includes Dijkstra, bidirectional Dijkstra, A*, bidirectional A*, Lee, IDA*, greedy variations, depth-first explorations, and more (see `Pathfinding.Domain.Core/Enums/Algorithms.cs`).
- **Pluggable heuristics and neighborhoods**: pick from multiple heuristic functions, step rules, and neighborhood shapes so you can experiment beyond the defaults.
- **Persistent experiments**: switch between LiteDB or SQLite stores, with both in-memory and on-disk options for quick tests or long-lived datasets.
- **Import/export**: move graphs, algorithm presets, and run histories between machines using binary, JSON, XML, or CSV bundles.

## Algorithms highlighted
- **Dijkstra & Bidirectional Dijkstra**: baseline cost-optimal search with optional two-frontier acceleration.
- **A* & Bidirectional A***: heuristic-guided shortest paths with dual-front search for larger maps.
- **Lee variants**: wave-propagation search for grid routing.
- **Iterative-deepening A*** and greedy/depth-first options for alternative exploration strategies.

## How to run (Terminal GUI)
1. Install the .NET 8 SDK.
2. From the repository root, launch the TUI:
   ```bash
   dotnet run --project src/Pathfinding.App.Console/Pathfinding.App.Console.csproj
   ```
3. Navigate with the keyboard to open graph editors, algorithm setups, and result viewers. Color themes can be adjusted via the settings panel.

## Data storage options
- **LiteDB**: choose in-memory for throwaway experiments or file-backed for durable runs.
- **SQLite**: use the built-in provider for cross-platform, file-based storage.

## Working with graphs and algorithms
- **Create graphs**: use the graph workspace to draw obstacles, set start/goal nodes, and choose neighborhood/step rules.
- **Define algorithm presets**: select an algorithm (e.g., Dijkstra, A*, Bidirectional) plus heuristics and smoothing options, then save the preset for reuse.
- **Run and compare**: execute saved presets against any graph to collect timing and step statistics.

## Importing & exporting
- **Supported formats**: binary (`.dat`), JSON (`.json`), XML (`.xml`), and CSV bundles (`.zip`).
- **What you can move**: single graphs, batches with ranges, or full run histories depending on the export option selected in the TUI.
- **Workflow**:
  1) open the export dialog from the graph or history view;
  2) pick the desired format and scope;
  3) confirm to save the file; use the matching import flow to load it elsewhere.

## Screenshots
_Add TUI captures here to showcase the graph editor, algorithm configuration dialog, and result visualizations._

## Project layout
- `src/Pathfinding.App.Console`: Terminal.Gui application with views, view models, and export/import flows.
- `src/Pathfinding.Domain.Core`: domain enums and shared primitives (algorithms, heuristics, neighborhoods, statuses).
- `src/Pathfinding.Infrastructure.Data`: LiteDB and SQLite repositories plus unit-of-work factories.
- `src/Pathfinding.Infrastructure.Business`: algorithm runners and business services.
- `tests/`: automated tests and benchmarks.

