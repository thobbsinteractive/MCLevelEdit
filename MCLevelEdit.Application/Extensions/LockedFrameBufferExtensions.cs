using Avalonia.Media;
using Avalonia.Platform;

namespace MCLevelEdit.Application.Extensions;

public static class LockedFramebufferExtensions
{
    public static Span<byte> GetPixels(this ILockedFramebuffer framebuffer)
    {
        unsafe
        {
            return new Span<byte>((byte*)framebuffer.Address, framebuffer.RowBytes * framebuffer.Size.Height);
        }
    }

    public static Span<byte> GetPixel(this ILockedFramebuffer framebuffer, int x, int y)
    {
        unsafe
        {
            var bytesPerPixel = framebuffer.Format.BitsPerPixel / 8;
            var zero = (byte*)framebuffer.Address;
            var offset = framebuffer.RowBytes * y + bytesPerPixel * x;
            return new Span<byte>(zero + offset, bytesPerPixel);
        }
    }

    public static void SetPixel(this ILockedFramebuffer framebuffer, int x, int y, Color color)
    {
        var pixel = framebuffer.GetPixel(x, y);
        pixel[0] = (byte)(color.R);
        pixel[1] = (byte)(color.G);
        pixel[2] = (byte)(color.B);
        pixel[3] = color.A;
    }
}
