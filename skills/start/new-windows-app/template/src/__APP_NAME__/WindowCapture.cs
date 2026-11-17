using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace __APP_NAME__;

/// <summary>
/// Saves the app's own window as a PNG. PrintWindow with PW_RENDERFULLCONTENT
/// draws the real window (title bar, Mica, navigation pane) and nothing of the
/// windows behind it. Falls back to RenderTargetBitmap, which leaves the backdrop blank.
/// </summary>
public static partial class WindowCapture
{
    /// <summary>Returns true for a real capture, false when it used the fallback.</summary>
    public static async Task<bool> SaveAsync(nint hwnd, FrameworkElement root, string path)
    {
        if (TryPrintWindow(hwnd, out var pixels, out var width, out var height))
        {
            await EncodeAsync(pixels, width, height, path);
            return true;
        }
        var rtb = new RenderTargetBitmap();
        await rtb.RenderAsync(root);
        var buffer = await rtb.GetPixelsAsync();
        await EncodeAsync(buffer.ToArray(), rtb.PixelWidth, rtb.PixelHeight, path);
        return false;
    }

    private static bool TryPrintWindow(nint hwnd, out byte[] pixels, out int width, out int height)
    {
        pixels = [];
        GetWindowRect(hwnd, out var window);
        // The visible frame, without the invisible resize border around it.
        if (DwmGetWindowAttribute(hwnd, DwmwaExtendedFrameBounds, out var frame, Marshal.SizeOf<Rect>()) != 0) frame = window;
        int fullWidth = window.Right - window.Left, fullHeight = window.Bottom - window.Top;
        width = frame.Right - frame.Left;
        height = frame.Bottom - frame.Top;
        if (width <= 0 || height <= 0) return false;

        var screen = GetDC(0);
        var dc = CreateCompatibleDC(screen);
        var bitmap = CreateCompatibleBitmap(screen, fullWidth, fullHeight);
        var old = SelectObject(dc, bitmap);
        var ok = PrintWindow(hwnd, dc, PwRenderFullContent);
        SelectObject(dc, old);

        var all = new byte[fullWidth * fullHeight * 4];
        var info = new BitmapInfoHeader { Size = 40, Width = fullWidth, Height = -fullHeight, Planes = 1, BitCount = 32 };
        GetDIBits(dc, bitmap, 0, (uint)fullHeight, all, ref info, 0);
        DeleteObject(bitmap);
        DeleteDC(dc);
        ReleaseDC(0, screen);
        if (!ok) return false;

        // Crop to the visible frame and make every pixel opaque (GDI leaves alpha at 0).
        pixels = new byte[width * height * 4];
        int left = frame.Left - window.Left, top = frame.Top - window.Top;
        var anyColor = false;
        for (var y = 0; y < height; y++)
        {
            Array.Copy(all, ((y + top) * fullWidth + left) * 4, pixels, y * width * 4, width * 4);
            for (var x = 0; x < width; x++)
            {
                var i = (y * width + x) * 4;
                pixels[i + 3] = 255;
                anyColor |= pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0;
            }
        }
        return anyColor;
    }

    private static async Task EncodeAsync(byte[] bgra, int width, int height, string path)
    {
        using var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, bgra);
        await encoder.FlushAsync();
        var bytes = new byte[stream.Size];
        stream.Seek(0);
        await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
        await File.WriteAllBytesAsync(path, bytes);
    }

    private const uint PwRenderFullContent = 2;
    private const int DwmwaExtendedFrameBounds = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint Size; public int Width, Height; public ushort Planes, BitCount;
        public uint Compression, SizeImage; public int XPelsPerMeter, YPelsPerMeter; public uint ClrUsed, ClrImportant;
    }

    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetWindowRect(nint hwnd, out Rect rect);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool PrintWindow(nint hwnd, nint dc, uint flags);
    [LibraryImport("user32.dll")] private static partial nint GetDC(nint hwnd);
    [LibraryImport("user32.dll")] private static partial int ReleaseDC(nint hwnd, nint dc);
    [LibraryImport("gdi32.dll")] private static partial nint CreateCompatibleDC(nint dc);
    [LibraryImport("gdi32.dll")] private static partial nint CreateCompatibleBitmap(nint dc, int width, int height);
    [LibraryImport("gdi32.dll")] private static partial nint SelectObject(nint dc, nint obj);
    [LibraryImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DeleteObject(nint obj);
    [LibraryImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DeleteDC(nint dc);
    [LibraryImport("gdi32.dll")] private static partial int GetDIBits(nint dc, nint bitmap, uint start, uint lines, [Out] byte[] bits, ref BitmapInfoHeader info, uint usage);
    [LibraryImport("dwmapi.dll")] private static partial int DwmGetWindowAttribute(nint hwnd, int attribute, out Rect value, int size);
}
