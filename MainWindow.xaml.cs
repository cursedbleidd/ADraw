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

namespace ADraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private Point lastPoint;
        private Brush penBrush;
        private double penThickness;
        private bool isImagePasting;
        private AModules modules;

        public MainWindow()
        {
            modules = new AModules();
            modules.LoadModules();
            InitializeComponent();
            if (modules.Modules.Any())
            {
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
            }
            else panel.Children.Remove(ToolsPop);

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
                    ToolsPop.IsOpen = !ToolsPop.IsOpen && modules.Modules.Any();
                    break;
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
    }
}
