﻿using CoreClassLib;
using MSA_password_kiosk_software.Controls;
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

namespace MSA_password_kiosk_software.Pages
{
    /// <summary>
    /// Interaction logic for ProcessingPage.xaml
    /// </summary>
    public partial class ProcessingPage : Page
    {
        private bool Successful { get; set; }
        public string Input { get; set; }

        public ProcessingPage(string input)
        {
            InitializeComponent();

            Input = input;
            //Set settings manually for debugging
            Core.UserIDLength = 5;
            Core.RandomPassword = true;
            Core.UsePrinter = true;
            //Hide the cursor
            Mouse.OverrideCursor = Cursors.None;
        }


        private async void ProcessRequest(string input)
        {
            //Start the circle animation
            progressBar.AnimateCircleAngle(0, 100, 2);
            var taskResult = await Task.Run(() => Core.ResetPassword(input, null));
            progressBar.StopAnimations();
            DisplayResult(taskResult);
        }

        private void DisplayResult(int returnValue)
        {
            //Password error scheme
            //Code 1 = User not found
            //Code 2 = Reset limit exceeded
            //Code 3 = Success
            //Code 4 = Unknown error
            //Code 5 = Account is disabled

            //Red = Software error
            //Orange = User error
            //Green = ~Success!

            switch(returnValue)
            {
                case 1: //User not found (sad)
                    progressBar.AnimateCircleColor(Colors.Red, 300);
                    progressBar.face.EmotionState = Controls.State.Sad;
                    resultLabel.Content = Texts.UserNotFoundMessage;
                    resultLabel.Foreground = Brushes.Red;
                    RoundProgressbar.FadeControlToOpacity(resultLabel, 1);
                    break;
                case 2: //Reset limit exceeded (neutral)
                    progressBar.AnimateCircleColor(Colors.Orange, 300);
                    progressBar.face.EmotionState = Controls.State.Neutral;
                    resultLabel.Content = Texts.ExceededLimitFailMessage;
                    resultLabel.Foreground = Brushes.Orange;
                    RoundProgressbar.FadeControlToOpacity(resultLabel, 1);
                    break;
                case 3: //Succesful (happy)
                    progressBar.AnimateCircleColor(Colors.Green, 300);
                    progressBar.face.EmotionState = Controls.State.Happy;
                    resultLabel.Content = Texts.SuccessMessage;
                    resultLabel.Foreground = Brushes.Green;
                    RoundProgressbar.FadeControlToOpacity(resultLabel, 1);
                    break;
                case 4: // No internet connection
                    progressBar.AnimateCircleColor(Colors.Red, 300);
                    progressBar.face.EmotionState = Controls.State.Sad;
                    resultLabel.Content = Texts.NoConnectErrorMessage;
                    resultLabel.Foreground = Brushes.Red;
                    RoundProgressbar.FadeControlToOpacity(resultLabel, 1);
                    break;
                case 5: // Account disabled
                    progressBar.AnimateCircleColor(Colors.Orange, 300);
                    progressBar.face.EmotionState = Controls.State.Neutral;
                    resultLabel.Content = Texts.AccountDisabledMessage;
                    resultLabel.Foreground = Brushes.Orange;
                    RoundProgressbar.FadeControlToOpacity(resultLabel, 1);
                    break;
            }
            //Exit screen animation
            DisplayDisappearMessage();
        }

        private void DisplayDisappearMessage()
        {
            disappearMessage.Content = Texts.DisappearMessage;
            RoundProgressbar.FadeControlToOpacity(disappearMessage, 1);
            progressBar.AnimateCircleAngle(100, 0, Core.FinalScreenShowTime, false, false);
            StartDisappearTimer();
        }

        private async void StartDisappearTimer()
        {
            int seconds = Core.FinalScreenShowTime;
            for(int i = seconds; i != 0; i--)
            {
                disappearMessage.Content = Texts.DisappearMessage.Replace("%", i.ToString());
                await Task.Delay(1000);
            }
            GoBackToMainPage();
        }

        private void GoBackToMainPage()
        {
            this.NavigationService.Navigate(new HomePage());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Opacity = 0;
            RoundProgressbar.FadeControlToOpacity(grid, 1);
            ProcessRequest(Input);
        }
    }
}
