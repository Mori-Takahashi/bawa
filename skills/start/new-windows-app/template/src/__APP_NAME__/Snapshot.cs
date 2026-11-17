using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using __APP_NAME__.UI;

namespace __APP_NAME__;

/// <summary>
/// Screenshot mode for agents (tools\snap.ps1 runs it):
///   APP_STORE=%TEMP%\__APP_NAME__-test.db APP_SEED=1 APP_SNAPSHOT=%TEMP%\__APP_NAME__-shots __APP_NAME__.exe
/// writes &lt;page&gt;-light.png and &lt;page&gt;-dark.png for every navigation page, and
/// settings-&lt;section&gt;-light.png and -dark.png for every Settings section, then quits.
/// Each PNG gets a -layout.json with the layout probes (tools\check-layout.cs).
/// APP_SNAPSHOT_PAGES=all,starred limits the pages; add "settings" to include Settings.
/// APP_ACCENT=&lt;id&gt; shows the pages in another accent.
/// For a dialog, flyout, or panel, add a capture here like the Settings loop.
/// </summary>
public static partial class Snapshot
{
    public static string? Folder => Environment.GetEnvironmentVariable("APP_SNAPSHOT") is { Length: > 0 } dir ? dir : null;

    public static async Task<int> RunAsync(MainWindow window)
    {
        var dir = Folder!;
        Directory.CreateDirectory(dir);
        var only = Environment.GetEnvironmentVariable("APP_SNAPSHOT_PAGES") is { Length: > 0 } list
            ? list.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : null;
        var failures = 0;
        await Task.Delay(1000);

        foreach (var (mode, theme) in new[] { ("light", ElementTheme.Light), ("dark", ElementTheme.Dark) })
        {
            window.SetThemeOverride(theme);
            foreach (var page in Enum.GetValues<SidebarPage>())
            {
                var id = page.ToString().ToLowerInvariant();
                if (only is not null && !only.Contains(id)) continue;
                window.ShowPage(page);
                if (!await CaptureAsync(window, Path.Combine(dir, $"{id}-{mode}.png"))) failures++;
            }
            if (only is null || only.Contains("settings"))
            {
                foreach (var section in Enum.GetValues<SettingsSection>())
                {
                    window.ShowSettings(section);
                    var id = section.ToString().ToLowerInvariant();
                    if (!await CaptureAsync(window, Path.Combine(dir, $"settings-{id}-{mode}.png"))) failures++;
                }
            }
        }
        AppOutput.Line($"SNAPSHOT done {dir}");
        return failures == 0 ? 0 : 1;
    }

    private static async Task<bool> CaptureAsync(MainWindow window, string path)
    {
        // Another app can minimize or cover this one. Bring it back to the front, as people see it.
        ShowWindow(window.Hwnd, SwRestore);
        SetForegroundWindow(window.Hwnd);
        window.Activate();
        window.FocusQuietly();
        await Task.Delay(800);
        try
        {
            File.WriteAllText(path.Replace(".png", "-layout.json"), Layout.Collect(window.RootElement).ToJson());
            var real = await WindowCapture.SaveAsync(window.Hwnd, window.RootElement, path);
            AppOutput.Line(real ? $"SNAPSHOT {path}" : $"SNAPSHOT {path} (fallback: backdrop and title bar are blank)");
            return true;
        }
        catch (Exception e)
        {
            AppOutput.Line($"SNAPSHOT failed {path}: {e.Message}");
            return false;
        }
    }

    private const int SwRestore = 9;
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool ShowWindow(nint hwnd, int command);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool SetForegroundWindow(nint hwnd);
}
