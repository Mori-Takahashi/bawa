using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Windows.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using __APP_NAME__.Pages;

namespace __APP_NAME__;

/// <summary>The main window: a navigation pane on the left, the page on the right, Mica behind both.</summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "__APP_NAME__";
        TitleText.Text = Title;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        // Mica on Windows 11. Windows 10 keeps the default background.
        if (MicaController.IsSupported()) SystemBackdrop = new MicaBackdrop();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 720));

        var settings = CommandUi.Accelerator(AppCommands.Settings);
        settings.Invoked += (_, e) => { ShowSettings(SettingsSection.Appearance); e.Handled = true; };
        Root.KeyboardAccelerators.Add(settings);

        App.Theme.Changed += ApplyTheme;
        ApplyTheme();
        ShowPage(SidebarPage.All);
    }

    public FrameworkElement RootElement => Root;

    public nint Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(this);

    /// <summary>True when the window has Mica, false when it uses the plain fallback background.</summary>
    public bool HasBackdrop => SystemBackdrop is not null;

    public void ShowPage(SidebarPage page)
        => Nav.SelectedItem = Nav.MenuItems.OfType<NavigationViewItem>().First(i => (string)i.Tag == page.ToString());

    public void ShowSettings(SettingsSection section)
    {
        Nav.SelectedItem = Nav.SettingsItem;
        (ContentFrame.Content as SettingsPage)?.Show(section);
    }

    /// <summary>Light, dark, or system, from Settings. Toggling the theme also refreshes accent brushes.</summary>
    public void ApplyTheme()
    {
        var theme = App.Theme.ElementTheme;
        Root.RequestedTheme = theme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        Root.RequestedTheme = theme;
        SetThemeOverride(theme);
    }

    /// <summary>Forces light or dark for one snapshot, without saving it.</summary>
    public void SetThemeOverride(ElementTheme theme)
    {
        Root.RequestedTheme = theme;
        AppWindow.TitleBar.PreferredTheme = theme switch
        {
            ElementTheme.Light => TitleBarTheme.Light,
            ElementTheme.Dark => TitleBarTheme.Dark,
            _ => TitleBarTheme.UseDefaultAppMode,
        };
        // The caption buttons (minimize, maximize, close) follow the app's theme, not Windows'.
        var dark = Root.ActualTheme == ElementTheme.Dark;
        var bar = AppWindow.TitleBar;
        bar.ButtonBackgroundColor = Colors.Transparent;
        bar.ButtonInactiveBackgroundColor = Colors.Transparent;
        bar.ButtonForegroundColor = dark ? Colors.White : Colors.Black;
        bar.ButtonHoverForegroundColor = dark ? Colors.White : Colors.Black;
        bar.ButtonHoverBackgroundColor = dark ? Color.FromArgb(0x15, 255, 255, 255) : Color.FromArgb(0x09, 0, 0, 0);
        bar.ButtonInactiveForegroundColor = dark ? Color.FromArgb(0x87, 255, 255, 255) : Color.FromArgb(0x72, 0, 0, 0);
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            if (ContentFrame.Content is not SettingsPage) ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }
        if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            App.Model.Page = Enum.Parse<SidebarPage>(tag);
            if (ContentFrame.Content is not ItemsPage) ContentFrame.Navigate(typeof(ItemsPage));
        }
    }
}
