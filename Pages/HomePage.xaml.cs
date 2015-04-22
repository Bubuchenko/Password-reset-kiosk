using CoreClassLib;
using MSA_password_kiosk_software.Controls;
using MSA_password_kiosk_software.Pages;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Opacity = 0;
            RoundProgressbar.FadeControlToOpacity(grid, 1);

            while(true)
            {
                if(!InputBox.IsFocused)
                {
                    InputBox.Focus();
                }

                await Task.Delay(50);
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                this.NavigationService.Navigate(new ProcessingPage("20523"));
            }
        }

        private void InputReceived(string input)
        {
            InputBox.Clear();

            //If input is valid, go to the next page
            if(Core.ValidateInput(input))
            {
                this.NavigationService.Navigate(new ProcessingPage(input));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ProcessingPage("123"));
        }
    }
}
