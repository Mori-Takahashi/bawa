using System.Runtime.InteropServices;
using System.Text;

namespace __APP_NAME__.Kit.Data;

/// <summary>
/// Where the database lives. The real one is %LOCALAPPDATA%\&lt;App&gt;\&lt;App&gt;.db.
/// APP_STORE overrides it. Snapshots, seed data, and the self-test only run
/// on a test store: a file under %TEMP%.
/// </summary>
public static class StorePath
{
    public static string Resolve(string appName, string? overridePath, string localAppData)
        => string.IsNullOrWhiteSpace(overridePath)
            ? Path.Combine(localAppData, appName, appName + ".db")
            : Path.GetFullPath(overridePath);

    /// <summary>True when <paramref name="path"/> is inside <paramref name="tempDir"/>,
    /// after resolving "..", relative parts, and short 8.3 names.</summary>
    public static bool IsTestStore(string path, string tempDir)
    {
        var file = Normalize(path);
        var temp = Normalize(tempDir).TrimEnd('\\') + "\\";
        return file.StartsWith(temp, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path)
    {
        var full = Path.GetFullPath(path);
        // Expand 8.3 names on the part that exists (the file itself may not exist yet).
        var existing = full;
        while (!string.IsNullOrEmpty(existing) && !Path.Exists(existing))
            existing = Path.GetDirectoryName(existing);
        if (string.IsNullOrEmpty(existing) || !OperatingSystem.IsWindows()) return full;
        var buffer = new StringBuilder(32767);
        var length = GetLongPathName(existing, buffer, buffer.Capacity);
        return length == 0 ? full : buffer.ToString() + full[existing.Length..];
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetLongPathName(string shortPath, StringBuilder longPath, int size);
}
