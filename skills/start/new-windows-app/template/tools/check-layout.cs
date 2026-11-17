#:project ../src/__APP_NAME__.Kit/__APP_NAME__.Kit.csproj
#:property PublishAot=false
// Checks the layout files that snapshot mode writes next to each PNG.
// Usage: dotnet run tools\check-layout.cs -- [--unit 4] "%TEMP%\__APP_NAME__-shots\*-layout.json"
// Wildcards are expanded here, because PowerShell passes them to dotnet as they are.
// --unit sets the grid step for rules 2 and 5. A design kit measured from
// another app keeps that app's numbers: pass its step, or 0.5 to check only
// edges, insets, and overlaps. The rules: src\__APP_NAME__.Kit\Layout\LayoutRules.cs.
// Prints PASS or FAIL per file and exits with 1 on any failure.
using System.Globalization;
using __APP_NAME__.Kit.Layout;

var paths = args.ToList();
var unit = 4.0;
var unitIndex = paths.IndexOf("--unit");
if (unitIndex >= 0 && unitIndex + 1 < paths.Count)
{
    unit = double.Parse(paths[unitIndex + 1], CultureInfo.InvariantCulture);
    paths.RemoveRange(unitIndex, 2);
}

var files = new List<string>();
foreach (var path in paths)
{
    if (path.Contains('*') || path.Contains('?'))
    {
        var dir = Path.GetDirectoryName(path) is { Length: > 0 } d ? d : ".";
        if (Directory.Exists(dir)) files.AddRange(Directory.GetFiles(dir, Path.GetFileName(path)).Order());
    }
    else files.Add(path);
}
if (files.Count == 0)
{
    Console.WriteLine("FAIL no layout files: run tools\\snap.ps1 first");
    return 1;
}

var failed = 0;
foreach (var file in files)
{
    var name = Path.GetFileName(file);
    LayoutFile layout;
    try { layout = LayoutFile.Parse(File.ReadAllText(file)); }
    catch (Exception e) { Console.WriteLine($"FAIL {name}: cannot read ({e.Message})"); failed++; continue; }

    var problems = LayoutRules.Check(layout, unit);
    var probes = layout.Probes.Count(p => p.Kind == "probe");
    var areas = layout.Probes.Count(p => p.Kind == "area");
    if (problems.Count == 0) Console.WriteLine($"PASS {name}: {probes} probes in {areas} areas");
    else
    {
        failed++;
        Console.WriteLine($"FAIL {name}");
        foreach (var problem in problems) Console.WriteLine($"  - {problem}");
    }
}
return failed == 0 ? 0 : 1;
