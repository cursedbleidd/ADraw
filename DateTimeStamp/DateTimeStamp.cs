using PluginInterface;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace DateTimeStamp
{
    [Version(2,5)]
    public class DateTimeStamp : IPlugin
    {
        public string Name
        {
            get
            {
                return "Дата и время";
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
            string currentDateTime = DateTime.Now.ToString();

            // Устанавливаем шрифт и размер для текста
            Font font = new Font("Arial", 12);

            // Устанавливаем цвет текста
            SolidBrush brush = new SolidBrush(Color.Red);

            // Создаем объект Graphics для рисования на изображении
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // Устанавливаем качество рисования
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                // Рисуем текст на изображении
                graphics.DrawString(currentDateTime, font, brush, new Point(bitmap.Width / 2 + 10, bitmap.Height - 20));
            }
        }
    }
}
