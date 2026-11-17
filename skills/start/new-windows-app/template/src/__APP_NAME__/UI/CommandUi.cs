using System.Windows.Input;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace __APP_NAME__;

/// <summary>Builds buttons and menu items from <see cref="AppCommands"/>, so every surface shows the same title, icon, and shortcut.</summary>
public static class CommandUi
{
    public static void Bind(AppBarButton button, AppCommand command)
    {
        button.Label = command.Title;
        button.Icon = new FontIcon { Glyph = command.Glyph };
        button.KeyboardAccelerators.Add(Accelerator(command));
        ToolTipService.SetToolTip(button, Tooltip(command));
        AutomationProperties.SetName(button, command.Title);
    }

    /// <summary>"Add an item to this list (Ctrl+N)": what it does, then the shortcut. Never the name again.</summary>
    public static string Tooltip(AppCommand command) => $"{command.Help} ({command.ShortcutText})";

    /// <summary>A menu item that shows the shortcut as text. The accelerator itself lives on the command bar button.</summary>
    public static MenuFlyoutItem MenuItem(AppCommand command, ICommand action, string? title = null) => new()
    {
        Text = title ?? command.Title,
        Icon = new FontIcon { Glyph = command.Glyph },
        Command = action,
        KeyboardAcceleratorTextOverride = command.ShortcutText,
    };

    public static KeyboardAccelerator Accelerator(AppCommand command)
        => new() { Key = command.Key, Modifiers = command.Modifiers };
}
