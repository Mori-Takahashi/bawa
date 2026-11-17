using System.Globalization;
using __APP_NAME__.Kit.Layout;
using Xunit;

namespace __APP_NAME__.Kit.Tests;

public class LayoutRulesTests
{
    private static LayoutProbe Area(string name, double x = 0, double w = 400, double inset = 24)
        => new("area", name, name, null, inset, x, 0, w, 600);

    private static LayoutProbe Probe(string name, double x, double y, double w = 100, double h = 20, string? edge = "lead", string area = "page")
        => new("probe", name, area, edge, 0, x, y, w, h);

    private static IReadOnlyList<string> Check(params LayoutProbe[] probes)
        => LayoutRules.Check(new LayoutFile(800, 600, probes));

    [Fact]
    public void Clean_layout_passes()
        => Assert.Empty(Check(Area("page"), Probe("title", 24, 24), Probe("list", 24, 56)));

    [Fact]
    public void Rule1_x_between_pixels_fails()
        => Assert.Contains(Check(Area("page"), Probe("title", 24.3, 24)), p => p.Contains("between pixels"));

    [Fact]
    public void Rule2_start_off_the_unit_fails()
        => Assert.Contains(Check(Area("page"), Probe("title", 26, 24, edge: null)), p => p.Contains("not a multiple of 4"));

    [Fact]
    public void Rule3_probe_inside_the_inset_fails()
        => Assert.Contains(Check(Area("page"), Probe("title", 8, 24)), p => p.Contains("inside its 24 px inset"));

    [Fact]
    public void Rule4_two_leading_edges_fail()
        => Assert.Contains(Check(Area("page"), Probe("title", 24, 24), Probe("list", 28, 56)), p => p.Contains("2 leading edges"));

    [Fact]
    public void Rule5_gap_off_the_unit_fails()
        => Assert.Contains(Check(Area("page"), Probe("title", 24, 24), Probe("list", 24, 50)), p => p.Contains("gap 6 px"));

    [Fact]
    public void Rule6_overlap_fails()
        => Assert.Contains(Check(Area("page"), Probe("a", 24, 24, edge: null), Probe("b", 48, 32, edge: null)), p => p.Contains("overlap"));

    [Fact]
    public void Rule7_shared_left_edge_across_areas_fails_when_different()
        => Assert.Contains(Check(Area("page"), Area("side", 400),
                Probe("t1", 24, 24, edge: "=left<"), Probe("t2", 428, 24, edge: "=left<", area: "side")),
            p => p.Contains("=left< has different left edges"));

    [Fact]
    public void Missing_area_fails()
        => Assert.Contains(Check(Probe("title", 24, 24)), p => p.Contains("has no Layout.Area"));

    [Fact]
    public void Unit_option_changes_the_step()
        => Assert.Empty(LayoutRules.Check(new LayoutFile(800, 600, [Area("page", inset: 0), Probe("t", 2, 0, edge: null)]), unit: 2));

    [Fact]
    public void Parses_layout_json_under_german_culture()
    {
        var saved = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var file = LayoutFile.Parse("""{"window":{"width":800.5,"height":600},"probes":[{"kind":"probe","name":"t","area":"page","edge":"lead","inset":0,"x":24.5,"y":0,"width":10,"height":10}]}""");
            Assert.Equal(800.5, file.WindowWidth);
            Assert.Equal(24.5, file.Probes[0].X);
        }
        finally { CultureInfo.CurrentCulture = saved; }
    }

    [Fact]
    public void Writes_json_with_a_dot_under_german_culture()
    {
        var saved = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var json = new LayoutFile(800.5, 600, [Probe("t", 24.5, 0)]).ToJson();
            Assert.Contains("800.5", json);
            Assert.Equal(24.5, LayoutFile.Parse(json).Probes[0].X);
        }
        finally { CultureInfo.CurrentCulture = saved; }
    }
}
