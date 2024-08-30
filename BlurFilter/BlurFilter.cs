using PluginInterface;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace BlurFilter
{
    [Version(5,11)]
    public class BlurFilter : IPlugin
    {
        public string Name
        {
            get
            {
                return "Размытие";
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
            float[,] filterMatrix = {
            { 0.111f, 0.111f, 0.111f },
            { 0.111f, 0.111f, 0.111f },
                { 0.111f, 0.111f, 0.111f }
            };

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Создаем временный массив для хранения промежуточных значений
            int[,] tempArray = new int[width, height];

            // Применяем фильтр к каждому пикселю изображения
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    // Инициализируем переменные для новых значений компонентов цвета
                    float red = 0, green = 0, blue = 0;

                    // Применяем матрицу фильтра к соседним пикселям
                    for (int filterX = 0; filterX < 3; filterX++)
                    {
                        for (int filterY = 0; filterY < 3; filterY++)
                        {
                            // Получаем координаты текущего пикселя
                            int imageX = x - 1 + filterX;
                            int imageY = y - 1 + filterY;

                            // Получаем цвет текущего пикселя
                            Color pixel = bitmap.GetPixel(imageX, imageY);

                            // Умножаем цвет текущего пикселя на соответствующий элемент матрицы фильтра
                            red += pixel.R * filterMatrix[filterX, filterY];
                            green += pixel.G * filterMatrix[filterX, filterY];
                            blue += pixel.B * filterMatrix[filterX, filterY];
                        }
                    }

                    // Обновляем временный массив с примененным фильтром значений цветов
                    tempArray[x, y] = Color.FromArgb((int)red, (int)green, (int)blue).ToArgb();
                }
            }

            // Копируем значения из временного массива обратно в изображение
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(tempArray[x, y]));
                }
            }
        }

    }
}
