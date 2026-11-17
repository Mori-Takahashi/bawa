using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using __APP_NAME__.Kit;
using __APP_NAME__.Kit.Data;

namespace __APP_NAME__.ViewModels;

/// <summary>The app's state: the current page, its items, and the selection. UI thread only.</summary>
public sealed partial class AppViewModel(ItemStore store) : ObservableObject
{
    public ItemStore Store { get; } = store;

    public ObservableCollection<Item> Items { get; } = [];

    [ObservableProperty]
    public partial SidebarPage Page { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleStarCommand), nameof(DeleteSelectionCommand))]
    public partial Item? Selection { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    partial void OnPageChanged(SidebarPage value) => Reload();

    /// <summary>Reads the items for the current page and keeps the selection when it still exists.</summary>
    public void Reload(long? select = null)
    {
        var keep = select ?? Selection?.Id;
        var items = Store.All().Where(i => Page != SidebarPage.Starred || i.IsStarred).ToList();
        Items.Clear();
        foreach (var item in items) Items.Add(item);
        IsEmpty = Items.Count == 0;
        Selection = Items.FirstOrDefault(i => i.Id == keep);
    }

    [RelayCommand]
    public void AddItem()
    {
        var item = Store.Add(ItemTitle.Clean("New item"));
        if (Page == SidebarPage.Starred) Store.SetStarred(item.Id, true);
        Reload(select: item.Id);
    }

    private bool HasSelection() => Selection is not null;

    [RelayCommand(CanExecute = nameof(HasSelection))]
    public void ToggleStar()
    {
        if (Selection is not { } item) return;
        Store.SetStarred(item.Id, !item.IsStarred);
        Reload(select: item.Id);
    }

    /// <summary>Deletes the selected item and selects the next one, so the keyboard keeps working.</summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    public void DeleteSelection()
    {
        if (Selection is not { } item) return;
        var index = Items.IndexOf(item);
        Store.Delete(item.Id);
        Reload();
        Selection = Items.Count == 0 ? null : Items[Math.Min(index, Items.Count - 1)];
    }
}
