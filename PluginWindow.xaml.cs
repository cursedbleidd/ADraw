using PluginInterface;
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
using System.Windows.Shapes;

namespace ADraw
{
    /// <summary>
    /// Логика взаимодействия для PluginWindow.xaml
    /// </summary>
    public partial class PluginWindow : Window
    {
        Dictionary<string, IPlugin> plugins;
        Dictionary<string, VersionAttribute> versions;
        public PluginWindow(Dictionary<string, IPlugin> plugins, Dictionary<string, VersionAttribute> versions)
        {
            this.plugins = plugins;
            this.versions = versions;
            InitializeComponent();
            foreach (string key in plugins.Keys)
            {
                lstBox.Items.Add(new ListBoxItem() { Content = $"Name: {plugins[key].Name} " +
                    $"Author: {plugins[key].Author} Version: {versions[key].Major}.{versions[key].Minor}"});
            }
        }
    }
}
