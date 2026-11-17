using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using __APP_NAME__.UI;

namespace __APP_NAME__.Pages;

/// <summary>Settings: one section per topic, picked in the selector bar at the top.</summary>
public sealed partial class SettingsPage : Page
{
    private bool _loading = true;

    public SettingsPage()
    {
        InitializeComponent();
        ThemeBox.SelectedIndex = (int)App.Theme.Theme;
        AccentBox.ItemsSource = ThemeStore.Accents;
        AccentBox.SelectedItem = ThemeStore.Accents.First(a => a.Id == App.Theme.AccentId);
        AppNameText.Text = "__APP_NAME__";
        VersionText.Text = "Version " + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0");
        _loading = false;
    }

    public SettingsSection Section { get; private set; } = SettingsSection.Appearance;

    public void Show(SettingsSection section)
        => Sections.SelectedItem = Sections.Items.First(i => (string)i.Tag == section.ToString());

    private void OnSectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        Section = Enum.Parse<SettingsSection>((string)sender.SelectedItem.Tag);
        AppearancePanel.Visibility = Section == SettingsSection.Appearance ? Visibility.Visible : Visibility.Collapsed;
        AboutPanel.Visibility = Section == SettingsSection.About ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        App.Theme.SetTheme((AppTheme)ThemeBox.SelectedIndex);
    }

    private void OnAccentChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || AccentBox.SelectedItem is not Accent accent) return;
        App.Theme.SetAccent(accent.Id);
    }
}
