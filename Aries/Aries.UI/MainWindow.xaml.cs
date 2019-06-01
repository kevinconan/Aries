using System;
using System.Windows;
using System.Windows.Controls;
using Aries.Model;
using MahApps.Metro.Controls;
using Aries.Lib;
using Microsoft.Win32;
using System.ComponentModel;

namespace Aries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        BindingList<ServerConfig> serverConfigs;

        public MapleStoryInspector inspector;

        public PortForwardingService portService;

        private bool isNetworkAdapterReady = false;

        public MainWindow()
        {
            InitializeComponent();

            LoadServerConfigs();

            InitMapleInspector();

            InitPortForwarding();

            InitNetworkAdapter();

        }

        //TODO: mock data
        private void LoadServerConfigs()
        {
            serverConfigs = ServerConfigService.LoadAll();

            cbServerConfig.DataContext = serverConfigs;

            cbServerConfig.SelectedValue = ServerConfigService.LastId;
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
            inspector.QuickPass = ServerConfigService.QuickPass;
            
        }

        private void InitNetworkAdapter()
        {
            NetworkAdapterInstaller.WarpMessage = WarpMessage;
            if (ServerConfigService.Mode == NetForwardMode.Adapter)
            {
                radio_Adapter.IsChecked = true;
                radio_Super.IsChecked = false;
            }else
            {
                radio_Super.IsChecked = true;
                radio_Adapter.IsChecked = false;
            }
            checkQuickPass.IsChecked = ServerConfigService.QuickPass;
            //NetworkAdapterInstaller.CheckAndInstallAdapter((bool success)=> {
            //    isNetworkAdapterReady = success;
            //});
        }

        private void InitPortForwarding()
        {
            portService = new PortForwardingService();
            portService.WarpMessage += new WarpMessage(WarpMessage);
        }
        private void LaunchForwarding(bool success)
        {
            Dispatcher.Invoke(() =>
            {
                if (success)
                {
                    ServerConfig cfg = serverConfigs[Convert.ToInt32(cbServerConfig.SelectedIndex)];
                    if (cfg.Host == "localhost" || cfg.Host == "127.0.0.1")
                    {
                        LauchMaple(success);
                        return;
                    }
                    portService.AddForwarding(8484, cfg.Host, cfg.LoginPort);
                    portService.AddForwarding(8600, cfg.Host, cfg.ShopPort);
                    portService.AddForwarding(8700, cfg.Host, cfg.AhPort);
                    portService.AddForwarding(8283, cfg.Host, cfg.ChatPort);
                    for (int i = cfg.ChannelStartPort, j = 0; i <= cfg.ChannelEndPort; i++, j++)
                    {
                        portService.AddForwarding(7575 + j, cfg.Host, cfg.ChannelStartPort+j);
                    }

                    portService.Launch(LauchMaple);
                }
                else
                {
                    SetStartBtn(false);
                }

            });
           
        }
        private void LauchMaple(bool success)
        {
            Dispatcher.Invoke(() =>
            {
                if (success)
                {
                    inspector.MapleStoryExe = (cbServerConfig.SelectedItem as ServerConfig).ExeLocation;
                    inspector.Launch();
                }
                else
                {
                    btnStart.IsEnabled = true;
                    btnStop.IsEnabled = false;
                }
                

            });
        }

        private void SetStartBtn(bool enable)
        {
            btnStart.IsEnabled = enable;
            CheckGroup.IsEnabled = enable;
            btnReset.IsEnabled = enable;
        }

        private void cbServerConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region 按钮事件
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            inspector.Stop();
            portService.Stop();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            

            SetStartBtn(false);
            ServerConfigService.LastId = (int)cbServerConfig.SelectedValue;
            if (isNetworkAdapterReady)
            {
                LaunchForwarding(isNetworkAdapterReady);
            }else
            {
                NetworkAdapterInstaller.CheckAndInstallAdapter(LaunchForwarding);
            }

        }

        private void radio_Adapter_Checked(object sender, RoutedEventArgs e)
        {
            SetStartBtn(false);

            NetworkAdapterInstaller.ChangeMode(NetForwardMode.Adapter, (bool success) => {
                Dispatcher.Invoke(() => {
                    SetStartBtn(true);
                    ServerConfigService.Mode = NetForwardMode.Adapter;
                });
            });
        }

        private void radio_Super_Checked(object sender, RoutedEventArgs e)
        {
            SetStartBtn(false);
            NetworkAdapterInstaller.ChangeMode(NetForwardMode.Route, (bool success) => {
                Dispatcher.Invoke(() => {
                    SetStartBtn(true);
                    ServerConfigService.Mode = NetForwardMode.Route;
                });
            });
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
                SetStartBtn(true);
                btnStop.IsEnabled = false;
            });

        }

        public void OnMapleStoryStartFail()
        {
            Dispatcher.Invoke(() =>
            {
                SetStartBtn(true);
                btnStop.IsEnabled = false;
            });
        }

        public void OnMapleStoryStartSuccess()
        {
            Dispatcher.Invoke(() =>
            {
                SetStartBtn(false);
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
                Dispatcher.Invoke(() => {

                    ServerConfig currentCfg = (ServerConfig)cbServerConfig.SelectedItem;
                    currentCfg.ExeLocation = fbd.FileName;
                    
                });
                return fbd.FileName;

            }

            return null;
        }
        #endregion

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            new EditServerConfigWindow().ShowDialog(this);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            new EditServerConfigWindow(cbServerConfig.SelectedItem as ServerConfig).ShowDialog(this);
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            new AriesAbout().ShowDialog(this);
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            
            
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ServerConfigService.RemoveFromMemory(cbServerConfig.SelectedItem as ServerConfig);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            ServerConfigService.SaveAll();
            NetworkAdapterInstaller.CloseNetwork();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            NetworkAdapterInstaller.ResetNetworkSettings();
            isNetworkAdapterReady = false;
        }

        private void checkQuickPass_Checked(object sender, RoutedEventArgs e)
        {
            ServerConfigService.QuickPass = (bool)checkQuickPass.IsChecked;
            inspector.QuickPass = ServerConfigService.QuickPass;
        }

        private void checkQuickPass_Unchecked(object sender, RoutedEventArgs e)
        {
            ServerConfigService.QuickPass = (bool)checkQuickPass.IsChecked;
            inspector.QuickPass = ServerConfigService.QuickPass;
        }
    }
}
