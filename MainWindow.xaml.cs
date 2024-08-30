using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Size = System.Windows.Size;
using Xceed.Wpf.AvalonDock.Controls;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using PluginInterface;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Microsoft.VisualBasic;

namespace ADraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private Point lastPoint;
        private System.Windows.Media.Brush penBrush;
        private double penThickness;
        private bool isImagePasting;
        private AModules modules;
        Dictionary<string, IPlugin> plugins;
        Dictionary<string, VersionAttribute> versions;

        public MainWindow()
        {
            plugins = new Dictionary<string, IPlugin>();
            versions = new Dictionary<string, VersionAttribute>();
            modules = new AModules();
            modules.LoadModules();
            InitializeComponent();
            foreach (KeyValuePair<string, AModules.Module> pair in modules.Modules)
                {
                    MenuItem item = new MenuItem();
                    item.Header = pair.Key;
                    item.Click += delegate (object sender, RoutedEventArgs e)
                    {
                        try
                        {
                            if (canvas == null)
                                return;

                            scrollvw.ScrollToHorizontalOffset(0);
                            scrollvw.ScrollToVerticalOffset(0);

                            Size size = new Size(canvas.ActualWidth, canvas.ActualHeight);
                            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
                            renderBitmap.Render(canvas);
                            BitmapSource bmpSource = BitmapFrame.Create(renderBitmap);

                            canvas.Children.Clear();

                            int width = bmpSource.PixelWidth;
                            int height = bmpSource.PixelHeight;
                            int stride = width * 4;
                            Int32[] PixelData = new Int32[width * height];
                            bmpSource.CopyPixels(PixelData, stride, 0);
                            unsafe
                            {
                                IntPtr pixelPtr = (IntPtr)Unsafe.AsPointer(ref PixelData[0]);
                                pair.Value.Func(pixelPtr, width, height);
                                BitmapSource newBmp = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Pbgra32, null, PixelData, stride);

                                canvas.Children.Add(new Image() { Source = newBmp });
                            }
                        }
                        catch (System.ArgumentOutOfRangeException)
                        { MessageBox.Show("No image"); }
                    };
                    stackTools.Children.Add(item);
                }
            FindPlugins();
            foreach (IPlugin p in  plugins.Values)
            {
                MenuItem item = new MenuItem();
                item.Header = p.Name;
                item.Click += delegate (object sender, RoutedEventArgs e)
                {

                    try
                    {
                        IPlugin plugin = plugins[((MenuItem)sender).Header.ToString()];

                        scrollvw.ScrollToHorizontalOffset(0);
                        scrollvw.ScrollToVerticalOffset(0);

                        Size size = new Size(canvas.ActualWidth, canvas.ActualHeight);
                        RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
                        renderBitmap.Render(canvas);
                        BitmapSource bmpSource = BitmapFrame.Create(renderBitmap);

                        canvas.Children.Clear();

                        Bitmap bmp = BitmapFromSource(bmpSource);
                        plugin.Transform(bmp);
                        bmpSource = ConvertBitmap(bmp);
                        canvas.Children.Add(new Image() { Source = bmpSource });
                    }
                    catch (System.ArgumentOutOfRangeException)
                    { MessageBox.Show("No image"); }
                };
                stackTools.Children.Add(item);
                MenuItem shItem = new MenuItem()
                {
                    Header = $"Name: {p.Name} " +
                        $"Author: {p.Author} Version: {versions[p.Name].Major}.{versions[p.Name].Minor}"
                };
                stackPlugins.Children.Add(shItem);
            }


            isDrawing = false;
            penBrush = Brushes.White;
            penThickness = 1;
            isImagePasting = false;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isImagePasting)
            {
                isDrawing = true;
                lastPoint = e.GetPosition(canvas);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {

            if (isDrawing && e.LeftButton == MouseButtonState.Pressed && !isImagePasting)
            {
                Point currentPoint = e.GetPosition(canvas);
                Line line = new Line
                {
                    X1 = lastPoint.X,
                    Y1 = lastPoint.Y,
                    X2 = currentPoint.X,
                    Y2 = currentPoint.Y,
                    StrokeThickness = slider.Value,
                };
                if (colorPicker.SelectedColor == null)
                    line.Stroke = Brushes.Black;
                else line.Stroke = new SolidColorBrush((Color)colorPicker.SelectedColor);
                canvas.Children.Add(line);
                lastPoint = currentPoint;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }


        private void PopupOpen(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Content)
            {
                case "File":
                    FilePop.IsOpen = !FilePop.IsOpen;
                    break;
                case "Pen":
                    PenPop.IsOpen = !PenPop.IsOpen;
                    break;
                case "Tools":
                    ToolsPop.IsOpen = !ToolsPop.IsOpen;
                    break;
                case "Plugins":
                    PluginsPop.IsOpen = !PluginsPop.IsOpen;

                    break;
            }
        }

        void FindPlugins()
        {
            // папка с плагинами
            string folder = System.AppDomain.CurrentDomain.BaseDirectory;
            string[] files = new string[0];
            bool isAuto = true;
            string debug;

            if (!File.Exists(folder + "config.cfg"))
            { 
                using (StreamWriter wr = new StreamWriter(folder + "config.cfg"))
                { wr.WriteLine("mode=auto"); }
            }
            else
            {
                using (StreamReader sr = new StreamReader(folder + "config.cfg"))
                {
                    string[] strings = sr.ReadToEnd().Split(new char[] { '\n', ' ', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    int i = 0;
                    for (; i < strings.Length; i++)
                        if (strings[i] == "mode" && strings[i + 1] == "=")
                        {
                            isAuto = strings[i + 2] == "auto";
                            i += 3;
                            break;
                        }
                    files = new string[strings.Length - i];
                    for (int j = 0; i < strings.Length; i++, j++) { files[j] = folder + strings[i]; }
                }
            }

            // dll-файлы в этой папке
            if (isAuto)
            {
                files = Directory.GetFiles(folder, "*.dll");
            }

            foreach (string file in files)
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);

                    foreach (Type type in assembly.GetTypes())
                    {
                        Type iface = type.GetInterface("PluginInterface.IPlugin");

                        if (iface != null)
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                            plugins.Add(plugin.Name, plugin);

                            VersionAttribute version = (VersionAttribute)Attribute.GetCustomAttribute(type, typeof(VersionAttribute));
                            versions.Add(plugin.Name, version);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки плагина\n" + ex.Message);
                }
        }


        private void OpenFile(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "PNG(*.PNG) | *.png";
                if (dialog.ShowDialog() == true)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(dialog.FileName, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    Image img = new Image() { Source = bitmapImage };
                    canvas.Children.Clear();
                    canvas.Height = bitmapImage.PixelHeight;
                    canvas.Width = bitmapImage.PixelWidth;
                    canvas.Children.Add(img);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "PNG(*.PNG) | *.png";
                if (dialog.ShowDialog() == true)
                {
                    Size size = new Size(canvas.ActualWidth, canvas.ActualHeight);
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
                    renderBitmap.Render(canvas);
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    using (var filestream = new FileStream(dialog.FileName, FileMode.Create))
                        pngEncoder.Save(filestream);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void CreateFile(object sender, RoutedEventArgs e)
        {
            if (height.Text == string.Empty || width.Text == string.Empty)
                return;
            canvas.Children.Clear();
            canvas.Height = Convert.ToInt32(height.Text);
            canvas.Width = Convert.ToInt32(width.Text);
            CreatePop.IsOpen = false;
        }

        private void PreviewText(object sender, TextCompositionEventArgs e)
        {
            string text = "0123456789";
            if (!text.Contains(e.Text))
                e.Handled = true;
        }

        private void CreateFilePop(object sender, RoutedEventArgs e)
        {
            CreatePop.IsOpen = !CreatePop.IsOpen;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            modules.Dispose();
        }
        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private void PluginsWindow(object sender, RoutedEventArgs e)
        {
            //doesnt work for some reason
            var wnd = new PluginWindow(plugins, versions);
            wnd.Show();
            //if (listBox.Items.IsEmpty)
            //{
            //    foreach (string key in plugins.Keys)
            //    {
            //        listBox.Items.Add(new ListBoxItem()
            //        {
            //            Content = $"Name: {plugins[key].Name} " +
            //            $"Author: {plugins[key].Author} Version: {versions[key].Major}.{versions[key].Minor}"
            //        });
            //    }
            //}
        }
    }
}
