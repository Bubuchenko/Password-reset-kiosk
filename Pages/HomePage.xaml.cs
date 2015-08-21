using CoreClassLib;
using MSA_password_kiosk_software.Controls;
using MSA_password_kiosk_software.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        //Store original color values
        Brush HeaderOriginalColorA;
        Brush HeaderOriginalColorB;
        Brush OriginalBackgroundColor;

        public HomePage()
        {
            InitializeComponent();

            HeaderOriginalColorA = TitleScreenText1.Foreground;
            HeaderOriginalColorB = TitleScreenText2.Foreground;

            //Hide the cursor
            Mouse.OverrideCursor = Cursors.None;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Opacity = 0;
            RoundProgressbar.FadeControlToOpacity(grid, 1);
            OriginalBackgroundColor = Window.GetWindow(this).Background;
            ScreenSafetyRefresh();

            while(true)
            {
                if(!InputBox.IsFocused)
                {
                    InputBox.Focus();
                }

                await Task.Delay(500);
            }
        }

        async void ScreenSafetyRefresh()
        {
            while (true)
            {
                try
                {
                    //n intervals
                    await Task.Delay(Core.ScreenProtectionRefreshInterval * 1000);
                    //Briefly change color to black and text to white
                    TitleScreenText1.Foreground = Brushes.Black;
                    TitleScreenText2.Foreground = Brushes.Black;
                    Window.GetWindow(this).Background = Brushes.White;
                    await Task.Delay(5000);
                    //Revert it back to the original color
                    TitleScreenText1.Foreground = HeaderOriginalColorA;
                    TitleScreenText2.Foreground = HeaderOriginalColorB;
                    Window.GetWindow(this).Background = OriginalBackgroundColor;
                }
                catch
                {
                    //Avoid throwing error if it has to refresh but we're not on the current page
                }
                
            }
        }


        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                InputReceived(InputBox.Text);
            }
        }

        private void InputReceived(string input)
        {
            InputBox.Clear();
            input = input.Trim();

            //If input is valid, go to the next page
            if(Core.ValidateInput(input))
            {
                this.NavigationService.Navigate(new ProcessingPage(input));
            }
        }
    }
}
