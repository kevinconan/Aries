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
using System.Windows.Shapes;
using Aries.Model;
using MahApps.Metro.Controls;
using Aries.Lib;

namespace Aries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        List<ServerConfig> serverConfigs;

        public MapleStoryInspector inspector;

        public MainWindow()
        {
            InitializeComponent();

            LoadServerConfigs();
        }

        //TODO: mock data
        private void LoadServerConfigs()
        {
            serverConfigs =new List<ServerConfig> {
                new ServerConfig
                {
                    ServerName="Test1",
                    Host="kevinconan.vicp.cc",
                    LoginPort=8484,
                    ShopPort=8600,
                    ChannelStartPort=7575,
                    ChannelEndPort=7580,
                    ExeLocation="MapleStory.exe",
                    ID=1
                },
                 new ServerConfig
                {
                    ServerName="Test2",
                    Host="127.0.0.1",
                    LoginPort=8484,
                    ShopPort=8600,
                    ChannelStartPort=7575,
                    ChannelEndPort=7580,
                    ExeLocation="MapleStory.exe",
                    ID=2
                }
            };

            DataContext = serverConfigs;
        }

        private void cbServerConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region 按钮事件
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

    }
}
