using System.Text.Json;
using System.Text.Json.Serialization;

namespace __APP_NAME__.Kit.Layout;

/// <summary>One marked view: an area (a page, a card) or a probe inside an area. Window pixels at scale 1, origin top left.</summary>
public sealed record LayoutProbe(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("area")] string Area,
    [property: JsonPropertyName("edge")] string? Edge,
    [property: JsonPropertyName("inset")] double Inset,
    [property: JsonPropertyName("x")] double X,
    [property: JsonPropertyName("y")] double Y,
    [property: JsonPropertyName("width")] double Width,
    [property: JsonPropertyName("height")] double Height)
{
    [JsonIgnore] public double MaxX => X + Width;
    [JsonIgnore] public double MaxY => Y + Height;
}

/// <summary>
/// The layout file snapshot mode writes next to each PNG. JSON numbers are
/// culture independent, so a German Windows writes and reads "800.5".
/// </summary>
public sealed record LayoutFile(double WindowWidth, double WindowHeight, IReadOnlyList<LayoutProbe> Probes)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static LayoutFile Parse(string json)
    {
        var dto = JsonSerializer.Deserialize<Dto>(json, Options) ?? throw new JsonException("empty layout file");
        return new LayoutFile(dto.Window.GetValueOrDefault("width"), dto.Window.GetValueOrDefault("height"), dto.Probes);
    }

    public string ToJson() => JsonSerializer.Serialize(new Dto(
        new Dictionary<string, double> { ["width"] = WindowWidth, ["height"] = WindowHeight },
        Probes.OrderBy(p => p.Area).ThenBy(p => p.Y).ThenBy(p => p.X).ToList()), Options);

    private sealed record Dto(
        [property: JsonPropertyName("window")] Dictionary<string, double> Window,
        [property: JsonPropertyName("probes")] List<LayoutProbe> Probes);
}
