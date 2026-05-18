# Calendar

> Version 0.6.0

---

## Stack

| Layer       | Technology                        | Version          |
|-------------|-----------------------------------|------------------|
| UI          | WinUI 3 / Windows App SDK         | 1.8.260204000    |
| Runtime     | .NET                              | 9.0              |
| MVVM        | CommunityToolkit.Mvvm             | 8.4.0            |
| Database    | SQLite via EF Core                | 9.0.2            |
| Build Tools | Microsoft.Windows.SDK.BuildTools  | 10.0.26100.7705  |
| Min OS      | Windows 10 (1903)                 | 10.0.17763.0     |

---

## Folder Structure

```
Calender/
├── App.xaml / App.xaml.cs              Entry point — DB init, sound state, theme restore
├── MainWindow.xaml / .cs               NavigationView shell + widget toggle
├── WidgetWindow.xaml / .cs             Frameless acrylic always-on-top widget window
├── app.manifest                        DPI-aware, Win10/11 compatibility
│
├── Models/
│   ├── CalendarEvent.cs                EF Core entity — persisted to SQLite
│   ├── CalendarDay.cs                  UI grid-cell model (ObservableObject)
│   └── AppSettings.cs                  Widget position, size, sound, and theme preferences
│
├── Data/
│   └── AppDbContext.cs                 SQLite context → %LOCALAPPDATA%\Calender\calendar.db
│
├── Services/
│   ├── CalendarService.cs              Async CRUD; overlap query for multi-day events
│   └── SettingsService.cs              JSON load/save → %LOCALAPPDATA%\Calender\settings.json
│
├── ViewModels/
│   ├── MainViewModel.cs                Shell-level state
│   ├── CalendarViewModel.cs            Month grid, navigation, CRUD commands
│   ├── AgendaViewModel.cs              60-day flat event list with date-group headers
│   ├── EventEditorViewModel.cs         Form state for the add/edit dialog
│   ├── SettingsViewModel.cs            Settings page state; applies theme and sound live
│   └── WidgetViewModel.cs              Shared widget VM; 5-min refresh timer, IDisposable
│
├── Views/
│   ├── CalendarPage.xaml / .cs         6-week grid — scrollable chips, multi-day arrows
│   ├── AgendaPage.xaml / .cs           Chronological list of upcoming events
│   ├── SettingsPage.xaml / .cs         Theme, sound, widget defaults, and about info
│   ├── EventEditorDialog.xaml / .cs    ContentDialog for creating and editing events
│   ├── SmallWidgetView.xaml / .cs      Compact widget — large date + TODAY/UPCOMING lists
│   ├── LargeWidgetView.xaml / .cs      Full widget — mini-calendar grid + TODAY events
│   └── AgendaTemplateSelector.cs       DataTemplateSelector for the agenda flat list
│
└── Converters/
    ├── BoolToAccentBrushConverter.cs   Accent-colour badge for today's date
    ├── BoolToOpacityConverter.cs       Dims padding days from adjacent months
    ├── BoolToVisibilityConverter.cs    Visibility toggle; supports ConverterParameter=Invert
    └── StringToColorConverter.cs       Hex string → SolidColorBrush for event chip dots
```

---



---

## Build & Run

### Prerequisites
- Visual Studio 2022 (v17.8+) with the **Windows application development** workload
- OR the Windows App SDK 1.8 runtime + .NET 9 SDK for CLI builds

### Visual Studio
1. Open `Calender.sln`
2. Set the solution platform to **x86** (required for the XAML compiler)
3. Press **F5**

### CLI
```bash
dotnet restore
dotnet build -c Debug
dotnet run --project Calender.csproj
```

---

## Data Locations

| File            | Path                                           |
|-----------------|------------------------------------------------|
| SQLite database | `%LOCALAPPDATA%\Calender\calendar.db`          |
| Settings JSON   | `%LOCALAPPDATA%\Calender\settings.json`        |

The database is created automatically on first launch. If it fails, restarting once fixes it.  
Inspect the database with [DB Browser for SQLite](https://sqlitebrowser.org/) or the VS Code SQLite extension.  
The data folder can be opened directly from the **Settings → About** section inside the app.

---


