using System.Drawing;
using PluginInterface;

namespace GrayScale
{
    [Version(3,2)]
    public class GrayScaleImage : IPlugin
    {
        public string Name
        {
            get
            {
                return "Оттенки серого";
            }
        }

        public string Author
        {
            get
            {
                return "Not Me";
            }
        }
        public void Transform(Bitmap bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color originalColor = bitmap.GetPixel(x, y);
                    int grayscaleValue = (int)(originalColor.R * 0.299 + originalColor.G * 0.587 + originalColor.B * 0.114);
                    Color grayscaleColor = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                    bitmap.SetPixel(x, y, grayscaleColor);
                }
            }
        }
    }
}