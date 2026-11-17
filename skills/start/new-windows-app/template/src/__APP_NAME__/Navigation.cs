using Windows.System;

namespace __APP_NAME__;

/// <summary>The pages in the navigation pane. Snapshot mode and the self-test visit every case.</summary>
public enum SidebarPage { All, Starred }

/// <summary>The sections of the Settings page, one per topic. Add a case for a new section.</summary>
public enum SettingsSection { Appearance, About }

/// <summary>One action: its title, Segoe Fluent Icons glyph, and keyboard shortcut.</summary>
public sealed record AppCommand(string Id, string Title, string Glyph, VirtualKey Key, VirtualKeyModifiers Modifiers)
{
    /// <summary>The shortcut as people read it, like "Ctrl+N".</summary>
    public string ShortcutText => (Modifiers.HasFlag(VirtualKeyModifiers.Control) ? "Ctrl+" : "")
        + (Modifiers.HasFlag(VirtualKeyModifiers.Shift) ? "Shift+" : "")
        + (Key == (VirtualKey)188 ? "," : Key.ToString());
}

/// <summary>
/// Every action in the app, in one list. Command bars, context menus, and
/// keyboard accelerators read from here, and the self-test checks that
/// every action has a shortcut and no shortcut is used twice.
/// </summary>
public static class AppCommands
{
    public static readonly AppCommand NewItem = new("new", "New item", "", VirtualKey.N, VirtualKeyModifiers.Control);
    public static readonly AppCommand ToggleStar = new("star", "Star", "", VirtualKey.D, VirtualKeyModifiers.Control);
    public static readonly AppCommand Delete = new("delete", "Delete", "", VirtualKey.Delete, VirtualKeyModifiers.None);
    public static readonly AppCommand Settings = new("settings", "Settings", "", (VirtualKey)188, VirtualKeyModifiers.Control);

    public static IReadOnlyList<AppCommand> All { get; } = [NewItem, ToggleStar, Delete, Settings];
}
