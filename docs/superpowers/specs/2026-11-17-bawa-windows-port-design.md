# BAWA: Umbau von BAMA auf Windows

Stand: 2026-11-17. Branch: `bawa-port`.

## Absicht

Das Repo wird von BAMA (Build Any Mac App) zu **BAWA: Build Any Windows App**.
Neue Beschreibung:

> Build Any Windows App. Agent skills to research, clone, design, build,
> optimize, and test native Windows apps with C# and WinUI 3.

Warum: Der Nutzer arbeitet auf Windows und will mit einem Agenten native
Windows-Apps bauen. Die Pipeline von BAMA (Idee, Research, Clone, Brief,
Stories, Gerüst, Design, Verbesserungsschleife) bleibt gleich. Werkzeuge,
Vorlage und Designregeln werden zu Windows, C# und WinUI 3.

Prüfstein: eine **Speicherplatz-App** für den PC des Nutzers. Sie scannt
Ordner und zeigt die größten Ordner, Dateien und installierten Programme,
damit man gezielt aufräumen kann. Meilenstein 1 dieser App wird mit den
umgebauten Skills gebaut. Was dabei hakt, fließt zurück in die Skills.

## Ziele

- Jeder ausgelieferte Skill funktioniert auf Windows 11 mit Claude Code
  (PowerShell 5.1 und Git Bash vorhanden, kein `pwsh`). Jeder Befehl, den ein
  Skill nennt, wurde hier mindestens einmal ausgeführt.
- Die Vorlage in `new-windows-app` baut und startet ohne Änderung. Sie hat
  Snapshot-Modus, Self-Test, Unit-Tests, Grid-Lint, Layout-Check und App-Icon.
- Die Regeln aus `.claude/CLAUDE.md` gelten weiter: Skills auf Englisch,
  Beschreibung als Kontext-Zeiger, jeder Schritt endet prüfbar, jeder Skill
  endet mit "Done when", keine Gedankenstriche.

## Nicht-Ziele

- Keine Mac-Unterstützung mehr in diesem Repo. BAMA bleibt das Original und
  wird in der README verlinkt.
- Kein MSIX-Paket und kein Store-Release in Phase 1.
- Keine plattformübergreifende App (kein MAUI, kein Avalonia, kein Electron).
- Kein WPF und kein WinForms als Standard. Sie tauchen nur als
  "bestehende App" in `build-and-verify/existing-app.md` auf.

## Anti-Ziele (wäre ein Fehlschlag, auch wenn es technisch läuft)

- Skills, die Befehle oder Skripte beschreiben, die es nicht gibt oder die
  auf Windows nicht laufen.
- Mac-Begriffe, die nur umbenannt wurden (Dock, Finder, SF Symbols,
  Command-Taste, Menüleiste als Pflicht).
- Eine Speicher-App, die Dateien ohne Rückfrage oder am Papierkorb vorbei
  löscht oder Systemordner anfasst.

## Vorgehen (gewählt: A)

| Weg | Inhalt | Abwägung |
| --- | --- | --- |
| **A. Kernpfad zuerst (gewählt)** | Erst die Skills, die die Speicher-App braucht, dann Meilenstein 1 der App bauen, dann die übrigen Skills | Jeder Skill ist erprobt, bevor er als fertig gilt. Eine Weile ist das Repo halb Mac, halb Windows |
| B. Alles auf einmal | Alle 21 Skills, dann die App | Schnell konsistent, aber Fehler der Vorlage zeigen sich erst am Ende |
| C. Erst Texte, dann Vorlage | Übersetzen, dann bauen | Verworfen: Texte beschreiben Werkzeuge, die es noch nicht gibt |

Der Nutzer hat A gewählt.

## Namen

| Vorher | Nachher |
| --- | --- |
| Plugin `bama` | `bawa` |
| `skills/start/bama` | `skills/start/bawa` |
| `new-mac-app` | `new-windows-app` |
| `swift-swiftui` | `csharp-winui` |
| `mac-design` | `windows-design` |
| `test-mac-app` | `test-windows-app` |

Alle anderen Skill-Namen bleiben. Jede Erwähnung in Skills, README,
`plugin.json`, `marketplace.json`, Router und `.claude/CLAUDE.md` /
`AGENTS.md` wird angepasst.

## Phasen

### Phase 1: Kernpfad

1. Repo-Rahmen: `plugin.json` und `marketplace.json` (Name, Beschreibung,
   Keywords, Repo `https://github.com/Mori-Takahashi/bawa`), README,
   `.claude/CLAUDE.md` und `AGENTS.md` (identisch), Router `bawa`.
   `plugin.json` listet nur Skills, die schon umgebaut sind.
2. `new-windows-app` mit der neuen Vorlage (siehe unten). Erst bauen und
   prüfen, dann die Skills, die sich darauf beziehen.
3. `build-and-verify`, `test-windows-app`, `layout-grid`.
4. `csharp-winui`, `windows-design`, `ux-design`.
5. `app-brief`, `user-stories`, `improve-loop`.
6. `specs/disk-space.md` (neue Spec im bestehenden Format: Brief, How it
   works, Stories, Gotchas, Done when) und `specs/README.md` mit nur dieser
   Spec, bis die übrigen umgebaut sind.
7. Prüfstein: Die Speicher-App wird in einem eigenen Ordner neben dem Repo
   gebaut (`C:\Users\lyone\Documents\<AppName>`), über den Router: `app-brief`
   (der Name wird dort mit dem Nutzer gewählt), `user-stories`,
   `new-windows-app`, `improve-loop` bis Meilenstein 1 fertig ist.
   Erkenntnisse werden als Änderungen an den Skills committet.
8. `version` in `plugin.json` auf `0.1.0`, `claude plugin validate . --strict`.

Phase 1 ist fertig, wenn Meilenstein 1 der Speicher-App läuft und alle
Skills aus Schritt 1 bis 6 validiert und auf Windows erprobt sind.

### Phase 2: der Rest

In dieser Reihenfolge, weil die Speicher-App sie in Meilenstein 2 braucht:
`performance`, `os-integrations`, `improve-architecture`, `design-system`,
`app-icon`, dann `research`, `clone`, `design-kit`, `design-from-app`,
`agent-cli`. Danach die übrigen Specs: jede wird auf Windows umgedacht oder
gestrichen (zum Beispiel ist `window-snapper` mit Snap Layouts schon
eingebaut, `menu-bar-stats` wird eine Tray-App). Dann `version` `1.0.0`.

Bis ein Skill umgebaut ist, bleibt er im Baum liegen, steht aber nicht in
`plugin.json`, nicht im Router und in der README nur als "kommt".

## Die Vorlage (`new-windows-app`)

### Aufbau

```text
<AppName>/
  <AppName>.slnx
  build.ps1                 dotnet publish -> %LOCALAPPDATA%\Programs\<AppName>, Startmenü-Verknüpfung
  CLAUDE.md                 Build-, Prüf- und Designregeln für Agenten
  Icon/AppIcon.svg          Quelle des Icons
  src/<AppName>/            die WinUI-3-App
    App.xaml(.cs)           Start, Datenbankort, Self-Test und Snapshot einhängen
    MainWindow.xaml(.cs)    NavigationView (Seitenleiste), eigene Titelleiste, Mica
    Pages/                  eine Page pro Seitenleisteneintrag, SettingsPage
    ViewModels/             CommunityToolkit.Mvvm
    Data/                   SQLite-Zugriff, Seed-Daten
    UI/Theme.xaml           Akzentfarben und Tokens als ThemeResources (Light/Dark/HighContrast)
    UI/Grid.cs              4-px-Skala (Space), Layout-Proben
    Snapshot.cs             APP_SNAPSHOT-Modus
    SelfTest.cs             --selftest
  src/<AppName>.Kit/        Logik ohne WinUI (net10.0), damit Tests ohne App laufen
  tests/<AppName>.Kit.Tests/  xUnit
  tools/
    snap.ps1  selftest.ps1  grid-lint.ps1
    check-layout.cs         dotnet run tools/check-layout.cs <json...>
    make-icon.cs            SVG -> Icon/AppIcon.ico (mehrere Größen)
```

### Technische Entscheidungen

Alle sind meine Wahl. Der Nutzer hat den Vorschlag gesehen und nicht
widersprochen. Paketversionen prüft der Builder beim Bauen auf den
aktuellen stabilen Stand.

- **.NET 10**, `net10.0-windows10.0.19041.0`, Windows App SDK (aktuelle
  stabile Version), **unpackaged** (`WindowsPackageType=None`) und
  `WindowsAppSDKSelfContained=true`, damit die exe ohne installierte Runtime
  läuft. Mindestens Windows 10 19041; das Design zielt auf Windows 11.
- **SQLite** über `Microsoft.Data.Sqlite` mit einer kleinen Tabelle
  `schema_version` für Migrationen. Neue Spalten bekommen einen Standardwert.
  Ort: `%LOCALAPPDATA%\<AppName>\<AppName>.db`. `APP_STORE` überschreibt ihn
  und muss unter `%TEMP%` liegen, sonst verweigern Snapshot, Seed und
  Self-Test.
- **CommunityToolkit.Mvvm** für ViewModels und Commands.
- **Skripte in PowerShell**, lauffähig unter Windows PowerShell 5.1.
  C#-Werkzeuge als Dateiprogramme mit `dotnet run <datei>.cs` (.NET 10).
- **Fenster**: `NavigationView` links als Seitenleiste, Einstellungen als
  eingebauter Settings-Eintrag unten (Windows-Konvention, kein eigenes
  Fenster). Eigene Titelleiste mit `ExtendsContentIntoTitleBar`, Mica als
  Hintergrund, auf Windows 10 ohne Mica.
- **Befehle**: Windows-Apps haben meist keine Menüleiste. Jede Aktion lebt
  in einer `CommandBar` oder einem Kontextmenü und hat ein
  `KeyboardAccelerator`. Der Self-Test prüft, dass jeder Befehl einen
  Shortcut hat und kein Shortcut doppelt ist.
- **Icons in der App**: Segoe Fluent Icons (`SymbolIcon`/`FontIcon`).
- **Snapshot-Modus**: `APP_SNAPSHOT=<ordner>` geht jede Seite und jeden
  Settings-Abschnitt in Light und Dark durch (`RequestedTheme` am Root),
  schreibt `<seite>-light.png`, `<seite>-dark.png` und je eine
  `-layout.json` mit den Proben, dann beendet sich die App.
  `APP_SNAPSHOT_PAGES` begrenzt die Seiten. Prüfbar: Die PNGs zeigen
  Titelleiste und Mica wie auf dem Bildschirm. Erste Wahl ist
  `PrintWindow` mit `PW_RENDERFULLCONTENT` auf das eigene Fenster,
  Rückfall `RenderTargetBitmap` mit einem Hinweis in der Ausgabe. Welche
  Methode Mica wirklich zeigt, klärt ein kurzer Test als erste Aufgabe.
- **Ausgabe**: Die App ist eine `WinExe` ohne Konsole. Self-Test und
  Snapshot schreiben ihre Zeilen in eine Logdatei neben `APP_STORE` und, wenn
  vorhanden, in die Elternkonsole (`AttachConsole`). `selftest.ps1` startet
  die App, wartet, gibt das Log aus und endet mit dem Exit-Code der App.
- **Self-Test**: `--selftest` führt die echten Funktionen der App auf einem
  Test-Store aus, druckt PASS oder FAIL pro Prüfung und endet mit 0 oder 1.
- **Grid**: 4-px-Raster (Fluent nutzt dasselbe). `grid-lint.ps1` prüft XAML
  und C# auf `Margin`, `Padding`, `Spacing`, `Width`, `Height`,
  `CornerRadius`, `RowSpacing`, `ColumnSpacing` außerhalb von 0, 1, 2 und
  Vielfachen von 4. Token-Dateien sind ausgenommen, `<!-- grid: ok <warum> -->`
  oder `// grid: ok <warum>` schaltet eine Zeile stumm.
- **Icon**: `Icon/AppIcon.svg` wird von `tools/make-icon.cs` (Svg.Skia per
  `#:package`) in eine `.ico` mit 16, 24, 32, 48, 64 und 256 px gerendert;
  `build.ps1` baut sie neu, wenn die SVG neuer ist.
- **Installieren**: `build.ps1` veröffentlicht nach
  `%LOCALAPPDATA%\Programs\<AppName>\`, legt eine Startmenü-Verknüpfung an,
  `-Open` beendet die laufende Kopie und startet die neue, `-Debug` baut
  schneller. Build-Ausgabe liegt außerhalb des Repos (`%LOCALAPPDATA%\<AppName>\build`),
  falls der Ordner in OneDrive liegt. Keine Signatur in Phase 1.
- **`new-app.ps1`**: kopiert die Vorlage, ersetzt `__APP_NAME__`, behält
  vorhandene Dateien (wie `docs/BRIEF.md`), führt `git init` aus und
  committet. Name in UpperCamelCase.

## Designregeln (`windows-design`)

Ersetzt Apples acht Prinzipien und die HIG durch Microsofts Fluent-Design-
und Windows-App-Richtlinien: Mica und Acrylic statt Liquid Glass (Mica für
den Fensterhintergrund, Acrylic nur für kurzlebige Flächen wie Flyouts),
Segoe UI Variable und die Fluent-Typografie (Body 14 px), Segoe Fluent Icons,
`NavigationView`, `CommandBar`, `ContentDialog` sparsam, `InfoBar` für
Meldungen, `TeachingTip`, Kontextmenüs, Tastatur und Zugriffstasten,
Light/Dark/Kontrast. `scripts/hig.sh` wird durch ein Skript ersetzt, das
die passende Seite der Windows-Design-Dokumentation (learn.microsoft.com)
als Text holt, oder gestrichen, wenn das nicht zuverlässig klappt.

## Speicherplatz-App (Prüfstein)

Der genaue Umfang entsteht im `app-brief` mit dem Nutzer. Feste Leitplanken:

- Löschen nur in den Papierkorb, immer mit Bestätigung, mit Liste und Größe.
- Systemordner (`C:\Windows`, `Program Files`, Benutzerprofil-Wurzel) sind
  sichtbar, aber nicht löschbar. Programme werden über ihren eigenen
  Deinstaller entfernt, nie durch Löschen des Ordners.
- Scans laufen im Hintergrund und ruckeln die Oberfläche nicht.

## Kleinere Entscheidungen (meine Wahl)

- `LICENSE` bleibt MIT. Daves Copyright-Zeile bleibt, eine Zeile für den
  Nutzer kommt dazu. Die README nennt BAMA als Ursprung und verlinkt es.
- Die Install-Befehle in der README zeigen auf `Mori-Takahashi/bawa`
  (`npx skills add Mori-Takahashi/bawa`, Plugin `bawa@bawa`).
- `scripts/link-skills.sh` wird `scripts/link-skills.ps1`. Es legt
  Verzeichnis-Junctions (`mklink /J`, ohne Adminrechte) statt Symlinks an.
- Jedes `agents/openai.yaml` bekommt einen Windows-Anzeigenamen und eine
  Windows-Kurzbeschreibung. Der Router behält
  `allow_implicit_invocation: false`.
- Der Unit-Test-Rahmen ist xUnit, ausgeführt mit `dotnet test`.

## Was beim Builder liegt

- Genaue Paketversionen, Struktur von `Grid.cs` und der Layout-Proben,
  Text der Skills im Einzelnen, Inhalte von `existing-app.md` für WPF und
  bestehende WinUI-Projekte.
- Ob `hig.sh` einen Windows-Ersatz bekommt.
- Banner-Bilder: Die alten BAMA-Banner fliegen raus. Die README startet mit
  einem Textkopf, bis es neue gibt.

## Autor und Herkunft (vom Nutzer entschieden)

- `plugin.json` und `marketplace.json`: Autor und Owner sind
  `Lyonel Berzen`, URL `https://github.com/Mori-Takahashi`.
- Das Original muss sichtbar bleiben:
  - Die README sagt gleich unter dem Kopf: "Based on
    [BAMA](https://github.com/daveebbelaar/bama) by Dave Ebbelaar" und hat
    einen Abschnitt "Credits" am Ende.
  - `LICENSE` behält "Copyright (c) 2026 Dave Ebbelaar" und bekommt
    darunter "Copyright (c) 2026 Lyonel Berzen".
  - Die `description` in `plugin.json` und `marketplace.json` endet mit
    "Based on BAMA by Dave Ebbelaar."
