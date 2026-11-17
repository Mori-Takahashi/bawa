using __APP_NAME__.Pages;
using Windows.System;

namespace __APP_NAME__;

/// <summary>The pages in the navigation pane. Snapshot mode and the self-test visit every case.
/// A new case needs its row in <see cref="NavPages"/>; the self-test checks that.</summary>
public enum SidebarPage { All, Starred }

/// <summary>One navigation entry: the page, its label, its glyph, and the Page class that shows it.</summary>
public sealed record NavPage(SidebarPage Page, string Title, string Glyph, Type PageType);

/// <summary>The one list the navigation pane is built from. Add a row here for every SidebarPage case.</summary>
public static class NavPages
{
    public static IReadOnlyList<NavPage> All { get; } =
    [
        new(SidebarPage.All, "All", "\uE8FD", typeof(ItemsPage)),
        new(SidebarPage.Starred, "Starred", "\uE734", typeof(ItemsPage)),
    ];
}

/// <summary>The sections of the Settings page, one per topic. Add a case for a new section.</summary>
public enum SettingsSection { Appearance, About }

/// <summary>
/// One action: its title, Segoe Fluent Icons glyph (write it as an escape like "\uE710"),
/// keyboard shortcut, and what it does for the tooltip.
/// </summary>
public sealed record AppCommand(string Id, string Title, string Glyph, VirtualKey Key, VirtualKeyModifiers Modifiers, string Help)
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
    public static readonly AppCommand NewItem = new("new", "New item", "\uE710", VirtualKey.N, VirtualKeyModifiers.Control, "Add an item to this list");
    public static readonly AppCommand ToggleStar = new("star", "Star", "\uE734", VirtualKey.D, VirtualKeyModifiers.Control, "Mark the selected item as starred");
    public static readonly AppCommand Delete = new("delete", "Delete", "\uE74D", VirtualKey.Delete, VirtualKeyModifiers.None, "Delete the selected item");
    public static readonly AppCommand Settings = new("settings", "Settings", "\uE713", (VirtualKey)188, VirtualKeyModifiers.Control, "Open Settings");

    public static IReadOnlyList<AppCommand> All { get; } = [NewItem, ToggleStar, Delete, Settings];
}
