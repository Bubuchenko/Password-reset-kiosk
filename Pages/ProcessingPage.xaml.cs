﻿using CoreClassLib;
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

namespace MSA_password_kiosk_software.Pages
{
    /// <summary>
    /// Interaction logic for ProcessingPage.xaml
    /// </summary>
    public partial class ProcessingPage : Page
    {
        private bool Successful { get; set; }

        public ProcessingPage(string input)
        {
            InitializeComponent();
            ProcessRequest(input);
        }

        private async void ProcessRequest(string input)
        {
            progressBar.AnimateCircleAngle(0, 100, 5);
            //Task task = Core.ResetPassword(input, null);

            Task<int> task = resetpassword();
            await task;

            progressBar.StopAnimations();
            DisplayResult(task.Result);
        }

        private void DisplayResult(int returnValue)
        {
            switch(returnValue)
            {
                case 1:
                    progressBar.AnimateCircleColor(Colors.Green);
                    break;
                case 2:
                    progressBar.AnimateCircleColor(Colors.Green);
                    break;
                case 3:
                    progressBar.AnimateCircleColor(Colors.Green);
                    break;
                case 4:
                    progressBar.AnimateCircleColor(Colors.Green);
                    break;
            }
        }

        async Task<int> resetpassword()
        {
            await Task.Delay(10000);
            return 2;
        }
    }
}
