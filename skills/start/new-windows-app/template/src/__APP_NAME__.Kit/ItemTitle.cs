using System.Text.RegularExpressions;

namespace __APP_NAME__.Kit;

/// <summary>Rules for item titles. Pure logic, tested in __APP_NAME__.Kit.Tests.</summary>
public static partial class ItemTitle
{
    /// <summary>Trims the title and turns every run of whitespace into one space.</summary>
    public static string Clean(string raw) => Whitespace().Replace(raw, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
