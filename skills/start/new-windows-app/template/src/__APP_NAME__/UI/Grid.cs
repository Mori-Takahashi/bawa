namespace __APP_NAME__.UI;

/// <summary>
/// The spacing scale, in effective pixels. Every margin, padding, spacing, and
/// size sits on a 4 px grid. XAML uses the same values from UI/Theme.xaml
/// (SpaceSm, PageMargin, ...). tools\grid-lint.ps1 flags numbers off the grid.
/// </summary>
public static class Space
{
    public const double Xs = 4;
    public const double Sm = 8;
    public const double Md = 12;
    public const double Lg = 16;
    public const double Xl = 20;
    public const double Xxl = 24;
    public const double Section = 32;
    /// <summary>Page margins.</summary>
    public const double Margin = 24;
}
