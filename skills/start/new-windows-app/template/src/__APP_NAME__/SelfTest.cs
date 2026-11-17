using Microsoft.UI.Composition.SystemBackdrops;
using __APP_NAME__.Kit;
using __APP_NAME__.ViewModels;

namespace __APP_NAME__;

/// <summary>
/// Self-test for agents: runs the app's real functions on a test store,
/// prints PASS or FAIL per check, and quits with exit code 0 or 1.
///   tools\selftest.ps1   (sets APP_STORE in %TEMP% and passes --selftest)
/// Add a check for every story with logic.
/// </summary>
public static class SelfTest
{
    public static bool Requested => Environment.GetCommandLineArgs().Contains("--selftest");

    private static int _count;
    private static int _failures;

    public static async Task<int> RunAsync(AppViewModel vm, MainWindow window)
    {
        await Task.Delay(300);
        window.ShowPage(SidebarPage.All);

        var before = vm.Store.Count();
        vm.AddItemCommand.Execute(null);
        Check("New item adds one item", vm.Store.Count() == before + 1);
        Check("the new item is selected", vm.Selection is not null);

        if (vm.Selection is { } item)
        {
            vm.ToggleStarCommand.Execute(null);
            Check("starring marks it starred", vm.Store.All().Single(i => i.Id == item.Id).IsStarred);
            vm.DeleteSelectionCommand.Execute(null);
            Check("deleting removes it", vm.Store.Count() == before);
        }
        Check("titles are cleaned", ItemTitle.Clean("  Buy   milk ") == "Buy milk");

        var missing = AppCommands.All.Where(c => c.Key == Windows.System.VirtualKey.None).Select(c => c.Title).ToList();
        Check("every command has a shortcut", missing.Count == 0, string.Join(", ", missing));
        var twice = AppCommands.All.GroupBy(c => (c.Key, c.Modifiers)).Where(g => g.Count() > 1)
            .Select(g => g.First().ShortcutText).ToList();
        Check("no shortcut is used twice", twice.Count == 0, string.Join(", ", twice));

        Check("window has a backdrop or a solid fallback", window.HasBackdrop || !MicaController.IsSupported());

        AppOutput.Line(_failures == 0 ? $"SELFTEST PASS {_count} checks" : $"SELFTEST FAIL {_failures} of {_count} checks");
        return _failures == 0 ? 0 : 1;
    }

    private static void Check(string name, bool ok, string detail = "")
    {
        _count++;
        if (!ok) _failures++;
        AppOutput.Line(ok ? $"PASS {name}" : $"FAIL {name}{(detail.Length > 0 ? ": " + detail : "")}");
    }
}
