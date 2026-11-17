using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using __APP_NAME__.Kit.Layout;

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

/// <summary>
/// Layout probes. Mark what you lay out yourself, and snapshot mode writes
/// their frames to &lt;page&gt;-&lt;mode&gt;-layout.json for tools\check-layout.cs:
///   ui:Layout.Area="page"  ui:Layout.Inset="24"      an area (a page, a card, a dialog)
///   ui:Layout.Probe="title"  ui:Layout.ProbeArea="page"  ui:Layout.Edge="lead"
/// Probes with the same Edge share one leading edge. Edge="" for views side by side.
/// Edge="=name&lt;" lines views up across areas (see LayoutRules).
/// In a list template, the first row's probe counts; later rows with the same name are skipped.
/// </summary>
public static class Layout
{
    public static readonly DependencyProperty AreaProperty = Attached("Area", "");
    public static readonly DependencyProperty InsetProperty = Attached("Inset", Space.Margin);
    public static readonly DependencyProperty ProbeProperty = Attached("Probe", "");
    public static readonly DependencyProperty ProbeAreaProperty = Attached("ProbeArea", "");
    public static readonly DependencyProperty EdgeProperty = Attached("Edge", "lead");

    public static string GetArea(DependencyObject o) => (string)o.GetValue(AreaProperty);
    public static void SetArea(DependencyObject o, string value) => o.SetValue(AreaProperty, value);
    public static double GetInset(DependencyObject o) => (double)o.GetValue(InsetProperty);
    public static void SetInset(DependencyObject o, double value) => o.SetValue(InsetProperty, value);
    public static string GetProbe(DependencyObject o) => (string)o.GetValue(ProbeProperty);
    public static void SetProbe(DependencyObject o, string value) => o.SetValue(ProbeProperty, value);
    public static string GetProbeArea(DependencyObject o) => (string)o.GetValue(ProbeAreaProperty);
    public static void SetProbeArea(DependencyObject o, string value) => o.SetValue(ProbeAreaProperty, value);
    public static string GetEdge(DependencyObject o) => (string)o.GetValue(EdgeProperty);
    public static void SetEdge(DependencyObject o, string value) => o.SetValue(EdgeProperty, value);

    private static DependencyProperty Attached(string name, object defaultValue)
        => DependencyProperty.RegisterAttached(name, defaultValue.GetType(), typeof(Layout), new PropertyMetadata(defaultValue));

    /// <summary>Every visible area and probe under <paramref name="root"/>, in window coordinates.</summary>
    public static LayoutFile Collect(FrameworkElement root)
    {
        var probes = new List<LayoutProbe>();
        var seen = new HashSet<string>();
        Walk(root, root, probes, seen);
        return new LayoutFile(root.ActualWidth, root.ActualHeight, probes);
    }

    private static void Walk(DependencyObject node, FrameworkElement root, List<LayoutProbe> probes, HashSet<string> seen)
    {
        if (node is UIElement { Visibility: Visibility.Collapsed }) return;
        if (node is FrameworkElement element && element.ActualWidth > 0)
        {
            var area = GetArea(element);
            var probe = GetProbe(element);
            if (area.Length > 0 && seen.Add("area:" + area)) probes.Add(Frame(element, root, "area", area, area, null, GetInset(element)));
            if (probe.Length > 0 && seen.Add("probe:" + probe))
            {
                var edge = GetEdge(element);
                probes.Add(Frame(element, root, "probe", probe, GetProbeArea(element), edge.Length == 0 ? null : edge, 0));
            }
        }
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            Walk(VisualTreeHelper.GetChild(node, i), root, probes, seen);
    }

    private static LayoutProbe Frame(FrameworkElement e, FrameworkElement root, string kind, string name, string area, string? edge, double inset)
    {
        var origin = e.TransformToVisual(root).TransformPoint(new Windows.Foundation.Point(0, 0));
        return new LayoutProbe(kind, name, area, edge, inset, origin.X, origin.Y, e.ActualWidth, e.ActualHeight);
    }
}
