using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aries.Lib;
using Aries.Model;
using MahApps.Metro.Controls;

namespace Aries
{
    /// <summary>
    /// Interaction logic for EditServerConfigWindow.xaml
    /// </summary>
    public partial class EditServerConfigWindow : MetroWindow
    {
        ServerConfig serverConfig;

        public EditServerConfigWindow(ServerConfig serverConfig)
        {
            InitializeComponent();

            this.serverConfig = serverConfig?.Clone() as ServerConfig ?? new ServerConfig();
            DataContext = this.serverConfig;
        }
        public EditServerConfigWindow() : this(new ServerConfig()) { }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            openFileDlg.Filter = "冒险岛主程序|MapleStory.exe";

            if (openFileDlg.ShowDialog(this) ?? false)
            {
                serverConfig.ExeLocation = openFileDlg.FileName;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ServerConfigService.SaveOrUpdateInMemory(serverConfig);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
