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

namespace MSA_password_kiosk_software
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            //Shortcuts to enter and leave fullscreen mode
            if (e.Key == Key.Home)
            {
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.Topmost = true;
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            if (e.Key == Key.End)
            {
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                this.Topmost = false;
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void MainFrame_KeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}
