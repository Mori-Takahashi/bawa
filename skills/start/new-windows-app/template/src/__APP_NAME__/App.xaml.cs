using Microsoft.UI.Xaml;
using __APP_NAME__.Data;
using __APP_NAME__.Kit.Data;
using __APP_NAME__.UI;
using __APP_NAME__.ViewModels;

namespace __APP_NAME__;

/// <summary>
/// Start-up: picks the database (APP_STORE overrides it), fills sample data
/// (APP_SEED=1, test stores only), and opens the main window.
/// </summary>
public partial class App : Application
{
    public const string Name = "__APP_NAME__";

    public static AppViewModel Model { get; private set; } = null!;
    public static ThemeStore Theme { get; private set; } = null!;
    public static MainWindow Window { get; private set; } = null!;

    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var overridePath = Environment.GetEnvironmentVariable("APP_STORE");
        var storePath = StorePath.Resolve(Name, overridePath,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        var isTestStore = !string.IsNullOrEmpty(overridePath) && StorePath.IsTestStore(storePath, Path.GetTempPath());

        var store = new ItemStore(storePath);
        if (Environment.GetEnvironmentVariable("APP_SEED") == "1" && isTestStore) SeedData.FillIfEmpty(store);

        Theme = new ThemeStore(Path.ChangeExtension(storePath, ".settings.json"));
        Theme.ApplyAccent();
        Model = new AppViewModel(store);
        Model.Reload();

        Window = new MainWindow();
        Window.Activate();
    }
}
