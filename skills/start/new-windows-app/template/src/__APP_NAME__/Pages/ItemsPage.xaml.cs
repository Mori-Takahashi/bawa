using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using __APP_NAME__.Kit.Data;
using __APP_NAME__.ViewModels;

namespace __APP_NAME__.Pages;

/// <summary>A list of items: All, or only the starred ones. One page serves both sidebar entries.</summary>
public sealed partial class ItemsPage : Page
{
    public AppViewModel ViewModel => App.Model;

    public ItemsPage()
    {
        InitializeComponent();
        CommandUi.Bind(NewButton, AppCommands.NewItem);
        CommandUi.Bind(StarButton, AppCommands.ToggleStar);
        CommandUi.Bind(DeleteButton, AppCommands.Delete);
        Loaded += (_, _) => { ViewModel.PropertyChanged += OnModelChanged; Refresh(); };
        Unloaded += (_, _) => ViewModel.PropertyChanged -= OnModelChanged;
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppViewModel.Page) or nameof(AppViewModel.Selection)) Refresh();
    }

    private void Refresh()
    {
        PageTitle.Text = ViewModel.Page.ToString();
        EmptyText.Text = ViewModel.Page == SidebarPage.Starred ? "No starred items" : "No items yet";
        if (!ReferenceEquals(List.SelectedItem, ViewModel.Selection)) List.SelectedItem = ViewModel.Selection;
        if (ViewModel.Selection is { } item) List.ScrollIntoView(item);
    }

    private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e)
        => ViewModel.Selection = List.SelectedItem as Item;

    /// <summary>Right-click or Shift+F10 on a row selects it and opens its menu.</summary>
    private void OnListContextRequested(UIElement sender, ContextRequestedEventArgs args)
    {
        if ((args.OriginalSource as FrameworkElement)?.DataContext is Item item) ViewModel.Selection = item;
        if (ViewModel.Selection is null) return;
        var menu = new MenuFlyout();
        menu.Items.Add(CommandUi.MenuItem(AppCommands.ToggleStar, ViewModel.ToggleStarCommand,
            ViewModel.Selection.IsStarred ? "Unstar" : null));
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(CommandUi.MenuItem(AppCommands.Delete, ViewModel.DeleteSelectionCommand));
        if (args.TryGetPosition(List, out var point)) menu.ShowAt(List, point);
        else menu.ShowAt(List);
        args.Handled = true;
    }
}
