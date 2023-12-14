using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MCLevelEdit.Application.Extensions;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Application.Utils;

public class BitmapUtils
{
    public static int index = 0;

    public static Task<bool> SaveBitmap(WriteableBitmap bitmap)
    {
        return Task.Run(() =>
        {
            try
            {
                string pathTempDir = Path.Combine(Path.GetTempPath(), Globals.APP_DIRECTORY);
                Directory.CreateDirectory(pathTempDir);

                using (var file = new FileStream(Path.Combine(pathTempDir, $"Temp{index}.png"), FileMode.Create))
                {
                    bitmap.Save(file);
                }
                index++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception saving png: {ex.Message}");
                return false;
            }

            return true;
        });
    }

    public static void SetBackground(Rect rect, Color colour, WriteableBitmap bitmap)
    {
        using (var fb = bitmap.Lock())
        {
            for (int x = (int)rect.X; x < rect.Width; x++)
            {
                for (int y = (int)rect.Y; y < rect.Height; y++)
                {
                    fb.SetPixel(x, y, colour);
                }
            }
        }
    }
}
