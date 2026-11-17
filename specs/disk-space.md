# Disk Space

A window that scans a drive or folder and shows what takes the space: the
biggest folders and files, sorted by size, and the installed programs with
their size. Selected items go to the Recycle Bin after a confirmation that
names them and how much space they free. Replaces: WinDirStat, TreeSize
Free, WizTree. Written for BAWA.

## Brief

```markdown
# DiskSpace

A window that shows what fills my drives, so I can free space safely.

## Replaces
WinDirStat, TreeSize Free, WizTree, and clicking through Explorer's folder properties

## Main jobs
1. Scan a drive or folder and see its biggest folders and files, biggest first.
2. See the installed programs with their size, and start their uninstaller.
3. Move what I pick to the Recycle Bin, knowing how much space it frees.

## Data
- Scan: root path, started at, finished at, total bytes, file count, folder count, folders it could not read.
- Node: path, name, is folder, size on disk, file count, last changed, children (folders only). Memory only.
- InstalledProgram: name, publisher, version, install date, size, install folder, uninstall command. Read from the registry, memory only.
- Settings: last root, show hidden files (default on), theme, accent. JSON next to the database.
- History: the last 10 scans (root, date, total) in SQLite, for the start page.

## Shape
Navigation window with three pages: Scan, Large files, Programs. Plus Settings.
Windows 10 (19041) or later, designed for Windows 11. Local data. Works offline. No accounts.

## System hooks
Folder picker, File Explorer (show in folder), the registry (installed programs),
the Recycle Bin, programs' own uninstallers.

## Reference
WinDirStat and TreeSize Free for the tree with size bars; Windows Settings > System >
Storage for the calm look.

## Look
Native Windows 11, system accent color. A tree with a size bar and percent per row,
like TreeSize, on Mica.

## Not in version one
- A treemap
- Duplicate file finder
- Store (MSIX) apps on the Programs page
- Scanning network drives
- Running as administrator
```

## How it works

| Part | API | Needs |
| --- | --- | --- |
| Pick a folder or drive | `FolderPicker` with `InitializeWithWindow` (`windows-design`, patterns.md) | None |
| List drives | `DriveInfo.GetDrives()` where `DriveType == Fixed` or `Removable` and `IsReady` | None |
| Walk the tree | `DirectoryInfo.EnumerateFileSystemInfos("*", options)`, one recursive walk, `EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = ReparsePoint, RecurseSubdirectories = false }` | None |
| File size | `FileInfo.Length` comes from the directory listing, no extra call per file | None |
| Size on disk | `GetCompressedFileSizeW` for compressed and sparse files (`FileAttributes.Compressed`, `SparseFile`) only | None |
| Online-only OneDrive files | Attribute `RecallOnDataAccess` (0x400000) or `Offline`: count 0 on disk, mark "online-only" | None |
| Folders it may not read | Count them while walking (`UnauthorizedAccessException` on the folder's own listing) | None |
| Installed programs | `Registry.LocalMachine` and `Registry.CurrentUser` under `SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`, plus `SOFTWARE\WOW6432Node\...` on 64-bit: `DisplayName`, `Publisher`, `DisplayVersion`, `InstallDate`, `EstimatedSize` (KB), `InstallLocation`, `UninstallString` | None |
| Uninstall | Start the program's `UninstallString` with `ProcessStartInfo { UseShellExecute = true }`; it asks for administrator rights itself | The uninstaller's own prompt |
| Move to the Recycle Bin | `Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile` / `DeleteDirectory` with `RecycleOption.SendToRecycleBin` (patterns.md) | None |
| Show in Explorer | `explorer.exe /select,"<path>"` | None |
| Drive free space | `DriveInfo.AvailableFreeSpace`, `TotalSize` | None |

Main types:

- `DiskScanner` (Kit): walks a root on a background thread, builds `Node`s,
  reports progress (folders done, current folder) at most 10 times a second,
  and stops on cancel. Returns a `ScanResult`.
- `Node` (Kit): path, name, size on disk, file count, children. Folder
  sizes are the sum of their children, computed once at the end of the walk.
- `ProtectedPaths` (Kit): says whether a path may be moved to the Recycle
  Bin (see Behaviors).
- `UninstallEntry` (Kit): parses a registry entry; `Parse(string)` splits an
  uninstall command into file and arguments.
- `SizeText` (Kit): "3.2 GB", "820 MB", "12 KB", "0 bytes".
- `ScanViewModel`, `LargeFilesViewModel`, `ProgramsViewModel`: one per page.

Behaviors that matter:

- **Scan.** The Scan page starts with the drives as cards (name, free and
  total space, a bar) and a "Choose folder..." button. Picking one starts
  the scan at once. During the scan: a progress bar without an end, the
  folder count, the current folder (trimmed in the middle), and Cancel.
  After it: the tree, the total, the time it took, and an InfoBar when
  folders could not be read ("12 folders could not be read. They need
  administrator rights. Their sizes are left out.").
- **Tree.** Each row: an expand chevron, a folder or file icon, the name, a
  size bar relative to its parent, the percent of the parent, the size, and
  the file count. Children sort biggest first. A folder shows its 100
  biggest children and one row "312 smaller items, 48 MB". Rows are
  created when a folder expands, never all at once.
- **Large files.** The 500 biggest files of the scan in a flat list: name,
  folder, size, last changed. Filter by type (Videos, Archives, Disk
  images, Installers, Other) with a combo box.
- **Programs.** All registry entries with a `DisplayName`, without
  `SystemComponent = 1` and without `ParentKeyName` (updates). Size from
  `EstimatedSize`; when it is missing and `InstallLocation` exists, the size
  of that folder, computed in the background. Sort by size. "Uninstall"
  starts the program's uninstaller and refreshes the list when it exits.
- **Recycle.** Select one or more rows (Ctrl and Shift click), then Delete
  or the command bar's "Move to Recycle Bin". The dialog names the item or
  the count and the total size, the safe button is the default. After it:
  the rows leave the tree, parent sizes drop, and an InfoBar says "14 items
  moved to the Recycle Bin (3.2 GB)" with "Open Recycle Bin"
  (`shell:RecycleBinFolder`).
- **Protected paths.** These can be scanned and shown but never moved to
  the Recycle Bin: the Windows folder, Program Files, Program Files (x86),
  ProgramData, the root of each drive, the user profile folder itself,
  `$Recycle.Bin`, `System Volume Information`, and any path that holds the
  running app. Their command bar button and menu item are disabled with a
  tooltip that says why. Folders inside the user profile (Downloads, Videos)
  are allowed.
- **Program folders.** A folder that is an installed program's
  `InstallLocation` shows "Installed program: <name>" and offers
  "Uninstall" instead of "Move to Recycle Bin".

## Stories

```markdown
## M1 Core loop

### S-001 See my drives
As the user, I open the app and see my drives with free and used space.
Screens: scan-start
Acceptance:
- [snap] The Scan page shows one card per fixed drive with name, letter, a bar, and "123 GB free of 476 GB".
- [unit] SizeText formats 0 as "0 bytes", 1536 as "1.5 KB", 3_435_973_837 as "3.2 GB".
- [self] The card list matches DriveInfo.GetDrives() filtered to ready fixed and removable drives.
Status: todo

### S-002 Scan a drive or folder
As the user, I pick a drive card or choose a folder, and the app scans it while staying responsive.
Depends: S-001
Acceptance:
- [unit] Scanning a test tree in %TEMP% (3 folders, 7 files, known sizes) gives the exact totals per folder.
- [unit] A junction inside the test tree is skipped, so nothing is counted twice.
- [self] Scanning the seeded test tree reports progress and finishes with the right total.
- [manual] While C:\ scans, the window moves, resizes, and Cancel stops the scan within a second.
Status: todo

### S-003 See the biggest folders first
As the user, I see the scanned folder as a tree, biggest first, with a size bar and percent per row.
Depends: S-002
Acceptance:
- [snap] The tree shows name, size bar, percent, size, and file count; children sort by size, biggest first.
- [unit] A folder with 150 children yields 100 rows plus "50 smaller items, <size>".
- [snap] The scan header shows the total, the file count, and the time the scan took.
Status: todo

### S-004 Know what could not be read
As the user, I see how many folders the scan could not read, instead of an error.
Depends: S-002
Acceptance:
- [unit] A folder without read rights in the test tree counts as 1 unread folder and 0 bytes.
- [snap] An InfoBar says "1 folder could not be read. It needs administrator rights. Its size is left out."
Status: todo

### S-005 Show an item in File Explorer
As the user, I right-click a row and pick "Show in File Explorer".
Depends: S-003
Acceptance:
- [manual] File Explorer opens the parent folder with the item selected.
- [self] The command exists in AppCommands with a shortcut (Ctrl+E).
Status: todo

### S-006 Rescan
As the user, I press F5 to scan the same folder again.
Depends: S-002
Acceptance:
- [self] F5 is in AppCommands; running the rescan command twice on the test tree gives the same totals.
- [ui] F5 starts a scan (the progress bar appears).
Status: todo

## M2 Find and uninstall

### S-007 See the largest files
As the user, I open Large files and see the 500 biggest files of the last scan.
Depends: S-002
Acceptance:
- [snap] Columns: name, folder, size, last changed; biggest first.
- [unit] The type filter puts .mp4 under Videos, .zip under Archives, .iso under Disk images, .msi and setup .exe under Installers.
Status: todo

### S-008 See installed programs by size
As the user, I open Programs and see every installed program with its size, biggest first.
Acceptance:
- [unit] A registry entry with SystemComponent = 1 or a ParentKeyName is left out.
- [unit] EstimatedSize 2048 (KB) shows as "2.0 MB".
- [snap] Rows show name, publisher, version, size, and an Uninstall button.
Status: todo

### S-009 Uninstall a program
As the user, I click Uninstall and the program's own uninstaller starts.
Depends: S-008
Acceptance:
- [unit] UninstallEntry.Parse splits `"C:\Program Files\App\unins000.exe" /SILENT` and `MsiExec.exe /X{GUID}` into file and arguments.
- [manual] The uninstaller starts; after it exits the list refreshes and the program is gone.
Status: todo

## M3 Free space

### S-010 Move items to the Recycle Bin
As the user, I select rows and press Delete; after I confirm, they go to the Recycle Bin.
Depends: S-003
Acceptance:
- [self] In the seeded test tree, recycling one folder removes it from disk and from the tree, and the parent size drops by its size.
- [snap] The dialog reads "Move 2 items to the Recycle Bin?" with the total size, buttons "Move to Recycle Bin" and "Cancel", Cancel is the default.
- [snap] After it, an InfoBar says "2 items moved to the Recycle Bin (1.2 MB)" with "Open Recycle Bin".
Status: todo

### S-011 Keep Windows and programs safe
As the user, I can never move system folders or the app itself to the Recycle Bin.
Depends: S-010
Acceptance:
- [unit] ProtectedPaths blocks C:\, C:\Windows, C:\Program Files, C:\ProgramData, the profile folder, $Recycle.Bin; it allows the profile's Downloads.
- [snap] On a protected row the command is disabled and its tooltip says why.
- [unit] A folder that is an installed program's InstallLocation offers Uninstall instead.
Status: todo

## Dropped
- Treemap view (later)
- Duplicate files (later)
- Store apps on the Programs page (needs package APIs, later)
- Network drives: their deletes skip the Recycle Bin
- Elevation: the app never asks to run as administrator
```

## Gotchas

- Junctions and symbolic links (`C:\Users\<you>\Application Data`, `C:\Documents and Settings`) point back into the tree. Following them loops or counts folders twice. Skip `ReparsePoint` entries while walking.
- `C:\Windows\WinSxS` is full of hard links: the same file shows up in several folders. Totals for the Windows folder are larger than the real space. Label that row "includes files shared with other folders" instead of trying to dedupe in version one.
- OneDrive files that are online-only report their full `Length` but use no space. Check `RecallOnDataAccess` (0x400000) and `Offline` attributes and count them as 0 on disk, or a OneDrive folder looks like it holds gigabytes it does not.
- `IgnoreInaccessible = true` hides access errors on files, but a folder whose own listing fails must still be counted as unread, or the user never learns sizes are missing.
- Paths over 260 characters fail on PCs without long paths enabled. The template's manifest opts in (`longPathAware`); count the rest as unread instead of crashing.
- `EstimatedSize` is in kilobytes, often missing, and sometimes wrong. Show "unknown" rather than 0 when it is missing and there is no install folder to measure.
- `UninstallString` comes in many shapes: quoted paths with arguments, unquoted paths with spaces, `MsiExec.exe /X{GUID}`, `rundll32` lines. Parse it in the Kit with table tests; never pass it to `cmd /c`.
- On network drives and some removable drives, "send to Recycle Bin" deletes for good. Version one does not scan network drives; for removable drives the dialog says "This drive has no Recycle Bin. The items will be deleted for good."
- A file in use cannot be moved. Report each failure in the InfoBar ("2 items are in use and stayed"), keep going with the rest.
- Updating the tree once per file freezes the window on a full drive (millions of files). Build the tree off the UI thread and show it when the scan ends; during the scan only the counters change.
- Scanning C:\ as a normal user takes one to several minutes on an SSD. Never scan at start; the user picks what to scan.

Done when scanning C:\ finishes without freezing the window, a test folder's total matches File Explorer's "Size on disk" within 1%, the Programs page lists what Settings > Apps lists for desktop programs, and nothing protected can reach the Recycle Bin.
