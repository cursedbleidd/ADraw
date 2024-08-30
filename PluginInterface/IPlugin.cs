using System.Drawing;

namespace PluginInterface
{
    public interface IPlugin
    {
            string Name { get; }
            string Author { get; }
            void Transform(Bitmap app);

    }
}