using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace __APP_NAME__.UI;

public enum AppTheme { System, Light, Dark }

/// <summary>One accent choice. "system" follows the Windows accent color.</summary>
public sealed record Accent(string Id, string Name, Color? Color);

/// <summary>
/// Light or dark mode and the accent color, saved as JSON next to the database.
/// APP_ACCENT=&lt;id&gt; overrides the accent for one run (compare accents in snapshots).
/// </summary>
public sealed class ThemeStore
{
    public static IReadOnlyList<Accent> Accents { get; } =
    [
        new("system", "Windows", null),
        new("blue", "Blue", Color.FromArgb(255, 0x00, 0x67, 0xC0)),
        new("purple", "Purple", Color.FromArgb(255, 0x87, 0x64, 0xB8)),
        new("pink", "Pink", Color.FromArgb(255, 0xC2, 0x39, 0xB3)),
        new("orange", "Orange", Color.FromArgb(255, 0xCA, 0x50, 0x10)),
        new("green", "Green", Color.FromArgb(255, 0x10, 0x7C, 0x10)),
    ];

    private readonly string _path;

    public AppTheme Theme { get; private set; } = AppTheme.System;
    public string AccentId { get; private set; } = "system";

    public ThemeStore(string path)
    {
        _path = path;
        try
        {
            if (File.Exists(path))
            {
                var saved = JsonSerializer.Deserialize<Saved>(File.ReadAllText(path));
                if (saved is not null)
                {
                    Theme = saved.Theme;
                    AccentId = Accents.Any(a => a.Id == saved.AccentId) ? saved.AccentId : "system";
                }
            }
        }
        catch (JsonException) { /* a broken file falls back to the defaults */ }
        var forced = Environment.GetEnvironmentVariable("APP_ACCENT");
        if (Accents.Any(a => a.Id == forced)) AccentId = forced!;
    }

    public event Action? Changed;

    public ElementTheme ElementTheme => Theme switch
    {
        AppTheme.Light => ElementTheme.Light,
        AppTheme.Dark => ElementTheme.Dark,
        _ => ElementTheme.Default,
    };

    public void SetTheme(AppTheme theme) { Theme = theme; Save(); Changed?.Invoke(); }

    public void SetAccent(string id) { AccentId = id; Save(); ApplyAccent(); Changed?.Invoke(); }

    /// <summary>Writes the accent into the app resources. Call before the first window and after a change.</summary>
    public void ApplyAccent()
    {
        var resources = Application.Current.Resources;
        var color = Accents.First(a => a.Id == AccentId).Color;
        string[] keys = ["SystemAccentColor", "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorLight3",
                         "SystemAccentColorDark1", "SystemAccentColorDark2", "SystemAccentColorDark3"];
        if (color is not { } c)
        {
            foreach (var key in keys) resources.Remove(key);
            return;
        }
        resources["SystemAccentColor"] = c;
        resources["SystemAccentColorLight1"] = Mix(c, Colors.White, 0.25);
        resources["SystemAccentColorLight2"] = Mix(c, Colors.White, 0.5);
        resources["SystemAccentColorLight3"] = Mix(c, Colors.White, 0.7);
        resources["SystemAccentColorDark1"] = Mix(c, Colors.Black, 0.2);
        resources["SystemAccentColorDark2"] = Mix(c, Colors.Black, 0.4);
        resources["SystemAccentColorDark3"] = Mix(c, Colors.Black, 0.6);
    }

    private static Color Mix(Color a, Color b, double t) => Color.FromArgb(255,
        (byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(new Saved(Theme, AccentId)));
    }

    private sealed record Saved(AppTheme Theme, string AccentId);
}
