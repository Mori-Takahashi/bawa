#:package FlaUI.UIA3@5.0.0
#:property PublishAot=false
#:property TargetFramework=net10.0-windows
// Drives the app from outside through UI Automation, the way Narrator and other tools see it.
// Usage: dotnet run tools\ui-check.cs -- <path to exe>
// It always runs the app on a fresh test store in %TEMP%, whatever the shell has set:
// the checks add and delete rows, and must never touch the user's real data.
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

var exe = args[0];
var appName = Path.GetFileNameWithoutExtension(exe);
foreach (var old in Directory.GetFiles(Path.GetTempPath(), $"{appName}-ui.*")) File.Delete(old);
var start = new ProcessStartInfo(exe) { UseShellExecute = false };
start.Environment["APP_STORE"] = Path.Combine(Path.GetTempPath(), $"{appName}-ui.db");
start.Environment.Remove("APP_SNAPSHOT");
var app = Application.Launch(start);
using var automation = new UIA3Automation();
var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }
// UI updates take a moment. Wait for the condition, up to 3 seconds, instead of a fixed sleep.
bool Within(Func<bool> condition)
{
    for (var end = DateTime.Now.AddSeconds(3); DateTime.Now < end; Thread.Sleep(100)) if (condition()) return true;
    return false;
}
try
{
    var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(15))!;
    var list = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.List))!.AsListBox();
    AutomationElement Button(string name) => window.FindFirstDescendant(cf => cf.ByName(name).And(cf.ByControlType(ControlType.Button)))!;

    // Patterns (Invoke, Select) work while the app is in the background.
    var before = list.Items.Length;
    Button("New item").Patterns.Invoke.Pattern.Invoke();
    Check("New item adds a row", Within(() => list.Items.Length == before + 1));
    list.Items[0].Patterns.SelectionItem.Pattern.Select();
    Button("Delete").Patterns.Invoke.Pattern.Invoke();
    Check("Delete removes the selected row", Within(() => list.Items.Length == before));

    // Real key presses only reach the window in front. Windows may refuse to bring
    // the app forward while someone else uses the PC, so check first and never type into another app.
    window.SetForeground();
    if (Within(() => automation.FocusedElement()?.Properties.ProcessId.ValueOrDefault == app.ProcessId))
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_N);
        Check("Ctrl+N adds a row", Within(() => list.Items.Length == before + 1));
    }
    else Console.WriteLine("SKIP Ctrl+N: the app is not in front");
}
finally
{
    app.Close();
}
return failures == 0 ? 0 : 1;
