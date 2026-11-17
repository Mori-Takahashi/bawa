using Microsoft.UI.Xaml;
using __APP_NAME__.Data;
using __APP_NAME__.Kit.Data;
using __APP_NAME__.UI;
using __APP_NAME__.ViewModels;

namespace __APP_NAME__;

/// <summary>
/// Start-up: picks the database (APP_STORE overrides it), fills sample data
/// (APP_SEED=1), and opens the main window. Self-test (--selftest) and
/// snapshots (APP_SNAPSHOT) run after the window is shown, then quit.
/// Seed data, self-test, and snapshots refuse to run on anything but a test store in %TEMP%.
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
        AppOutput.Init(isTestStore ? storePath + ".log" : null);

        var mode = SelfTest.Requested ? "SELFTEST"
            : !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APP_SNAPSHOT")) ? "SNAPSHOT"
            : Environment.GetEnvironmentVariable("APP_SEED") == "1" ? "SEED"
            : null;
        if (mode is not null && !isTestStore)
        {
            // Never touch the real database: refuse before opening any store.
            AppOutput.Line($"{mode} refused: set APP_STORE to a file in %TEMP%");
            Quit(1);
            return;
        }

        var store = new ItemStore(storePath);
        if (Environment.GetEnvironmentVariable("APP_SEED") == "1") SeedData.FillIfEmpty(store);

        Theme = new ThemeStore(Path.ChangeExtension(storePath, ".settings.json"));
        Theme.ApplyAccent();
        Model = new AppViewModel(store);
        Model.Reload();

        Window = new MainWindow();
        Window.Activate();

        if (SelfTest.Requested)
            Window.RootElement.Loaded += async (_, _) => Quit(await SelfTest.RunAsync(Model, Window));
    }

    /// <summary>Quits with an exit code that scripts read.</summary>
    public static void Quit(int code)
    {
        Environment.ExitCode = code;
        Current.Exit();
    }
}
