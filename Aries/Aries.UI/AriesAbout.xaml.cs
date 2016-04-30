using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using System.Diagnostics;

namespace Aries
{
    /// <summary>
    /// Interaction logic for AriesAbout.xaml
    /// </summary>
    public partial class AriesAbout : MetroWindow
    {
        public AriesAbout()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(Window window)
        {
            this.Owner = window;
            return ShowDialog();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
