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
using System.Threading;
using Microsoft.Win32;

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

            InitMapleInspector();

            DataContext = new Dictionary<string, object>();
            tbLogs.DataContext = "";
        }

        //TODO: mock data
        private void LoadServerConfigs()
        {
            serverConfigs = new List<ServerConfig> {
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

            cbServerConfig.DataContext = serverConfigs;
        }

        private void InitMapleInspector()
        {
            inspector = new MapleStoryInspector();
            inspector.OnMapleStoryStartSuccess += new OnMapleStoryStartSuccess(OnMapleStoryStartSuccess);
            inspector.OnMapleStoryStartFail += OnMapleStoryStartFail;
            inspector.OnMapleStoryShutdown += OnMapleStoryShutdown;
            inspector.OnMapleStoryWindowChange += new OnMapleStoryWindowChange(OnMapleStoryWindowChange);
            inspector.WarpMessage += new WarpMessage(WarpMessage);
            inspector.GetMapleMainPath += new GetMapleMainPath(GetMapleMainPath);

        }

        private void cbServerConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region 按钮事件
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            inspector.Stop();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            inspector.MapleStoryExe = (cbServerConfig.SelectedItem as ServerConfig).ExeLocation;
            inspector.Launch();
        }
        #endregion

        #region MapleInspector代理
        public void OnMapleStoryWindowChange(MapleStoryWindowType windowType)
        {

        }

        public void OnMapleStoryShutdown()
        {
            Dispatcher.Invoke(() =>
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
            });

        }

        public void OnMapleStoryStartFail()
        {
            Dispatcher.Invoke(() =>
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
            });
        }

        public void OnMapleStoryStartSuccess()
        {
            Dispatcher.Invoke(() =>
            {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
            });
        }

        public void WarpMessage(MessageType type, string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                tbLogs.Text += (type == MessageType.Tips ? "[信息]" : "[错误]") + message + "\n";
                tbLogs.ScrollToEnd();
            });
            //tbLogs.DataContext += type == MessageType.Tips ? "[信息]" : "[错误]"+message+"\n";
        }

        public string GetMapleMainPath()
        {
            OpenFileDialog fbd = new OpenFileDialog();
            fbd.Filter = "冒险岛主程序|MapleStory.exe";
            //fbd.InitialDirectory = readMapleRegInf();
            if (fbd.ShowDialog() == true)
            {

                return fbd.FileName;
            }

            return null;
        }
        #endregion

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            new EditServerConfigWindow().ShowDialog();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            new EditServerConfigWindow(cbServerConfig.SelectedItem as ServerConfig).ShowDialog();
        }
    }
}
