using System.Globalization;

namespace __APP_NAME__.Kit.Layout;

/// <summary>
/// Checks a layout file against the grid. Rules, for views marked with
/// ui:Layout.Area and ui:Layout.Probe (UI/Grid.cs):
///   1. Every view starts on a whole or half pixel across, so edges stay sharp.
///   2. A probe starts a multiple of the unit (4 px) from its area's leading edge.
///   3. A probe stays at least the area's inset from both sides.
///   4. Probes with the same area and edge share one leading edge.
///   5. Stacked probes on one edge are a multiple of the unit apart (text heights
///      have fractions, so gaps get 0.5 px of room).
///   6. Probes in one area never overlap.
///   7. Probes whose edge starts with "=" line up across areas, and rules 2 to 6 skip them:
///      "=name" same left and right, "=name&lt;" same left, "=name&gt;" same right,
///      "=name|" same center, "=name_" same bottom.
/// The unit is 4 by default. A design kit measured from another app passes its own step.
/// </summary>
public static class LayoutRules
{
    private const double Tolerance = 0.26;
    private const double GapTolerance = 0.51;
    private const double PixelTolerance = 0.05;

    public static IReadOnlyList<string> Check(LayoutFile file, double unit = 4)
    {
        var problems = new List<string>();
        var areas = file.Probes.Where(p => p.Kind == "area").GroupBy(p => p.Area).ToDictionary(g => g.Key, g => g.First());
        var probes = file.Probes.Where(p => p.Kind == "probe" && !(p.Edge ?? "").StartsWith('=')).ToList();

        // Rule 7
        foreach (var group in file.Probes.Where(p => (p.Edge ?? "").StartsWith('=')).GroupBy(p => p.Edge!).OrderBy(g => g.Key))
        {
            if (group.Count() < 2) continue;
            void Differ(string what, Func<LayoutProbe, double> value)
            {
                if (group.Max(value) - group.Min(value) > Tolerance)
                    problems.Add($"{group.Key} has different {what}: {string.Join(", ", group.Select(p => $"{p.Name} {F(value(p))}"))}");
            }
            switch (group.Key[^1])
            {
                case '<': Differ("left edges", p => p.X); break;
                case '>': Differ("right edges", p => p.MaxX); break;
                case '|': Differ("centers", p => p.X + p.Width / 2); break;
                case '_': Differ("bottoms", p => p.MaxY); break;
                default: Differ("left edges", p => p.X); Differ("right edges", p => p.MaxX); break;
            }
        }

        // Rule 1
        foreach (var probe in file.Probes.Where(p => p.Width > 0))
            if (!OnGrid(probe.X, 0.5, PixelTolerance)) problems.Add($"{probe.Name}: x {F(probe.X)} is between pixels");

        // Rules 2 and 3
        foreach (var probe in probes)
        {
            if (!areas.TryGetValue(probe.Area, out var area))
            {
                problems.Add($"{probe.Name}: area \"{probe.Area}\" has no Layout.Area");
                continue;
            }
            var lead = probe.X - area.X;
            if (!OnGrid(lead, unit)) problems.Add($"{probe.Name}: starts {F(lead)} px into {area.Name}, not a multiple of {F(unit)}");
            if (lead < area.Inset - Tolerance) problems.Add($"{probe.Name}: starts {F(lead)} px into {area.Name}, inside its {F(area.Inset)} px inset");
            var trail = area.MaxX - probe.MaxX;
            if (trail < area.Inset - Tolerance) problems.Add($"{probe.Name}: ends {F(trail)} px from the right of {area.Name}, inside its {F(area.Inset)} px inset");
        }

        // Rules 4 and 5
        foreach (var group in probes.Where(p => p.Edge is not null).GroupBy(p => $"{p.Area}/{p.Edge}").OrderBy(g => g.Key))
        {
            var xs = group.Select(p => Math.Round(p.X * 2) / 2).Distinct().Count();
            if (xs > 1) problems.Add($"edge {group.Key} has {xs} leading edges: {string.Join(", ", group.Select(p => $"{p.Name} {F(p.X)}"))}");
            var stacked = group.OrderBy(p => p.Y).ToList();
            for (var i = 1; i < stacked.Count; i++)
            {
                var (above, below) = (stacked[i - 1], stacked[i]);
                if (below.Y < above.MaxY - Tolerance) continue;
                var gap = below.Y - above.MaxY;
                if (!OnGrid(gap, unit, Math.Min(GapTolerance, unit / 2 - 0.01)))
                    problems.Add($"{above.Name} to {below.Name}: gap {F(gap)} px, not a multiple of {F(unit)}");
            }
        }

        // Rule 6
        foreach (var group in probes.GroupBy(p => p.Area))
        {
            var list = group.ToList();
            for (var i = 0; i < list.Count; i++)
                for (var j = i + 1; j < list.Count; j++)
                {
                    var (a, b) = (list[i], list[j]);
                    var overlapX = Math.Min(a.MaxX, b.MaxX) - Math.Max(a.X, b.X);
                    var overlapY = Math.Min(a.MaxY, b.MaxY) - Math.Max(a.Y, b.Y);
                    if (overlapX > Tolerance && overlapY > Tolerance) problems.Add($"{a.Name} and {b.Name} overlap in {group.Key}");
                }
        }
        return problems;
    }

    private static bool OnGrid(double value, double step, double within = Tolerance)
    {
        var rest = Math.IEEERemainder(value, step);
        return Math.Abs(rest) < within;
    }

    private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
