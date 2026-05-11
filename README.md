# Calendar app


> Version 0.3.0


---

## INFORMATION

### Technology Stack

| Layer | Technology | Version |
|---|---|---|
| UI Framework | WinUI 3 / Windows App SDK | 1.8.260209005 |
| Runtime | .NET | 9.0 |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 |
| Database | SQLite via EF Core | 9.0.2 |
| Build Tools | Microsoft.Windows.SDK.BuildTools | 10.0.26100.7705 |
| Min OS | Windows 10 (1903) | 10.0.17763.0 |

### Folder Structure

```
Calender/
├── App.xaml / App.xaml.cs              Entry point; registers converters, initialises DB
├── MainWindow.xaml / .cs               NavigationView shell + PaneFooter widget toggle
├── WidgetWindow.xaml / .cs             Frameless acrylic always-on-top widget window
├── app.manifest                        DPI-aware, Win10/11 compatibility
│
├── Models/
│   ├── CalendarEvent.cs                EF Core entity — persisted to SQLite
│   ├── CalendarDay.cs                  UI grid-cell model (ObservableObject, not persisted)
│   └── AppSettings.cs                  Widget position / size preference model
│
├── Data/
│   └── AppDbContext.cs                 SQLite context → %LOCALAPPDATA%\Calender\calendar.db
│
├── Services/
│   ├── CalendarService.cs              Async CRUD over AppDbContext
│   └── SettingsService.cs              JSON load/save → %LOCALAPPDATA%\Calender\settings.json
│
├── ViewModels/
│   ├── MainViewModel.cs                Shell-level state
│   ├── CalendarViewModel.cs            Month grid, nav commands, CRUD commands
│   ├── EventEditorViewModel.cs         Form state for the add/edit dialog
│   └── WidgetViewModel.cs              Shared widget VM; 5-min refresh timer, IDisposable
│
├── Views/
│   ├── CalendarPage.xaml / .cs         6-week grid with event chips + slide animation
│   ├── EventEditorDialog.xaml / .cs    ContentDialog for creating and editing events
│   ├── SmallWidgetView.xaml / .cs      Compact widget — large date + TODAY/UPCOMING lists
│   └── LargeWidgetView.xaml / .cs      Full widget — mini-calendar grid + TODAY events
│
└── Converters/
    ├── BoolToAccentBrushConverter.cs   Accent-colour badge for today's date
    ├── BoolToOpacityConverter.cs       Dims padding days from adjacent months
    ├── BoolToVisibilityConverter.cs    Visibility toggle; supports ConverterParameter=Invert
    └── StringToColorConverter.cs       Hex string → SolidColorBrush for event chip dots
```
---

## How to test

### Prerequisites
- Visual Studio 2022 (v17.8+) with the **Windows application development** workload installed
- OR the Windows App SDK 1.8 runtime installed separately if running from CLI
- .NET 9.0 SDK (`dotnet --version` should report `9.x`)

### Build & Run (Visual Studio)
1. Open `Calender.sln`
2. Set the solution platform to **x64** (top toolbar)
3. Press **F5** to build and launch with the debugger

### Build & Run (CLI)
```bash
# From the repository root
dotnet restore
dotnet build -c Debug -r win-x64
dotnet run --project Calender.csproj -r win-x64
```


### Database
The SQLite database is created automatically on first launch at:
```
%LOCALAPPDATA%\Calender\calendar.db
```
Sometimes it doesn't create on the first launch — restarting the app usually fixes it.
To inspect the database open the file with [DB Browser for SQLite](https://sqlitebrowser.org/) or the VS Code SQLite extension.

### Settings
Widget position and size are stored at:
```
%LOCALAPPDATA%\Calender\settings.json
```

---

