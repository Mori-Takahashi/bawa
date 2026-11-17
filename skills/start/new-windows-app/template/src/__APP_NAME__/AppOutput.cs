using System.Runtime.InteropServices;

namespace __APP_NAME__;

/// <summary>
/// Output for agents. The app has no console window, so each line goes to
/// stdout (a pipe or the console that started it) and to a log file next to
/// the test store, flushed per line so a crash keeps the lines before it.
/// </summary>
public static partial class AppOutput
{
    private static string? _logPath;

    public static void Init(string? logPath)
    {
        _logPath = logPath;
        // Started from a console without redirection: write into that console.
        AttachConsole(-1);
    }

    public static void Line(string text)
    {
        try { Console.WriteLine(text); Console.Out.Flush(); } catch (IOException) { }
        if (_logPath is not null) File.AppendAllText(_logPath, text + Environment.NewLine);
    }

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachConsole(int processId);
}
