#:package Svg.Skia@5.2.3
#:property PublishAot=false
// Renders the SVG app icon to a Windows .ico at 16, 24, 32, 48, 64, and 256 px:
// 32-bit bitmaps for the small sizes (every reader supports them), PNG for 256.
// Usage: dotnet run tools\make-icon.cs -- Icon\AppIcon.svg Icon\AppIcon.ico
// build.ps1 runs it when the SVG is newer than the .ico.
using SkiaSharp;
using Svg.Skia;

if (args.Length != 2)
{
    Console.WriteLine("usage: dotnet run tools\\make-icon.cs -- <icon.svg> <icon.ico>");
    return 1;
}
var (source, target) = (args[0], args[1]);
using var svg = new SKSvg();
if (svg.Load(source) is not { } picture)
{
    Console.WriteLine($"ICON cannot read {source}");
    return 1;
}

int[] sizes = [16, 24, 32, 48, 64, 256];
var images = new List<byte[]>();
foreach (var size in sizes)
{
    using var bitmap = new SKBitmap(size, size, SKColorType.Bgra8888, SKAlphaType.Unpremul);
    using (var canvas = new SKCanvas(bitmap))
    {
        canvas.Clear(SKColors.Transparent);
        var bounds = picture.CullRect;
        canvas.Scale(size / bounds.Width, size / bounds.Height);
        canvas.DrawPicture(picture);
    }
    images.Add(size >= 256 ? Png(bitmap) : Dib(bitmap, size));
}

static byte[] Png(SKBitmap bitmap)
{
    using var image = SKImage.FromBitmap(bitmap);
    using var png = image.Encode(SKEncodedImageFormat.Png, 100);
    return png.ToArray();
}

// A DIB icon image: BITMAPINFOHEADER (height doubled for the mask), BGRA rows bottom-up, then an empty AND mask.
static byte[] Dib(SKBitmap bitmap, int size)
{
    var maskRow = (size + 31) / 32 * 4;
    using var stream = new MemoryStream();
    using var w = new BinaryWriter(stream);
    w.Write(40); w.Write(size); w.Write(size * 2); w.Write((ushort)1); w.Write((ushort)32);
    w.Write(0); w.Write(size * size * 4 + maskRow * size); w.Write(0); w.Write(0); w.Write(0); w.Write(0);
    var pixels = bitmap.GetPixelSpan();
    for (var y = size - 1; y >= 0; y--) w.Write(pixels.Slice(y * size * 4, size * 4));
    w.Write(new byte[maskRow * size]);
    w.Flush();
    return stream.ToArray();
}

// ICO: a 6-byte header, a 16-byte entry per image, then the PNG data.
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(target))!);
using (var output = new BinaryWriter(File.Create(target)))
{
    output.Write((ushort)0);
    output.Write((ushort)1);
    output.Write((ushort)sizes.Length);
    var offset = 6 + 16 * sizes.Length;
    for (var i = 0; i < sizes.Length; i++)
    {
        output.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
        output.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
        output.Write((byte)0);
        output.Write((byte)0);
        output.Write((ushort)1);
        output.Write((ushort)32);
        output.Write(images[i].Length);
        output.Write(offset);
        offset += images[i].Length;
    }
    foreach (var image in images) output.Write(image);
}
Console.WriteLine($"ICON {target} ({sizes.Length} sizes)");
return 0;
