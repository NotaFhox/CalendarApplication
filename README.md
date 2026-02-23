# Calendar app 
YES I SPELT CALENDAR WRONG ITS TOO LATE TO FIX IT NOW THIS IS THE PLACEHOLDER NAME

> **Version 0.1.0** — Initial project scaffold
> Updated: 2026-02-23

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


Calender/
├── App.xaml / App.xaml.cs          
├── MainWindow.xaml / .cs           
├── app.manifest                    
│
├── Models/
│   ├── CalendarEvent.cs            
│   └── CalendarDay.cs              
│
├── Data/
│   └── AppDbContext.cs             
│
├── Services/
│   └── CalendarService.cs          
│
├── ViewModels/
│   ├── MainViewModel.cs            
│   └── CalendarViewModel.cs        
│
├── Views/
│   ├── CalendarPage.xaml           
│   └── CalendarPage.xaml.cs        
│
└── Converters/
    ├── BoolToAccentBrushConverter  
    └── BoolToOpacityConverter      


### Key Design Decisions
- Unpackaged deployment
     (`WindowsPackageType=None`) — no MSIX/Package.appxmanifest 
- MicaBackdrop applied at Window level
     allows glass effect to cover the full shell including NavigationView pane
- EnsureCreated on app startup 
    creates schema automatically without needing migrations

## How to test

### Prerequisites
- Visual Studio 2022 (v17.8+) with the `Windows application development` workload installed
- OR the Windows App SDK 1.8 runtime installed separately if running from CLI
- .NET 9.0 SDK (`dotnet --version` should report `9.x`)

### Build & Run (Visual Studio)
1. Open `Calender.sln`
2. Set the solution platform to x64 (top toolbar)
3. Press F5 to build and launch with the debugger

### Build & Run (CLI)
```bash
# From the repository root
dotnet restore
dotnet build -c Debug -r win-x64
dotnet run --project Calender.csproj -r win-x64
```

### Database
The SQLite database should be created automatically on first launch at:
```
%LOCALAPPDATA%\Calender\calendar.db
```
Sometimes it doesnt do this, No one actually knows why x3
To inspect it for whatever reason open the file with [DB Browser for SQLite](https://sqlitebrowser.org/) or the VS Code SQLite extension.



## PROBLEMS FOUND

Issues identified in the current v0.1.0

| # | Severity | Location | Description |
|---|---|---|---|

 1 | Medium | `Converters/BoolToAccentBrushConverter.cs` | Accent color is resolved via a manual `Resources.TryGetValue` lookup at convert-time rather than binding to `SystemAccentColorBrush` directly

| 2 | Low | `Views/CalendarPage.xaml` | `{x:Bind Date.Day}` in the DataTemplate binds an `int` to `TextBlock.Text` (a `string`); relies on implicit `ToString()` coercion in WinUI 3

| 3 | Low | `MainWindow.xaml.cs` | `"AgendaPage"` nav item maps to `CalendarPage` as a placeholder

| 4 | Low | Project name | The root namespace and assembly are named `Calender` (missing the 'a').

| 5 | Info | `App.xaml.cs` | `InitializeDatabase()` calls `EnsureCreated()` synchronously on the UI thread at startup

| 6 | Info | `CalendarViewModel.cs` | `_ = LoadEventsAsync()` fire-and-forget pattern discards exceptions silently; add a `try/catch` or a `.ContinueWith` error handler before the service layer is actively used |

