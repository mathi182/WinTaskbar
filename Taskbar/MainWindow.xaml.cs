﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Taskbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifiable
    {
        private List<ApplicationButton> appButtons = new List<ApplicationButton>();
        private Chronometer chronometer = new Chronometer();

        public MainWindow()
        {
            InitializeComponent();

            appButtons.Add(new ApplicationButton { Button = btnTaskManager, IconPath = @"C:\Windows\system32\taskmgr.exe" });

            Background = Brushes.Gray;
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
            Left = 0;
            chronometer.AddNotifiable(this);
            chronometer.SetFrequency(TimeSpan.FromSeconds(0.5));

            SetupChronoButtons();
            SetupAppButtons();
        }

        private void SetupAppButtons()
        {
            int btnCount = 0;
            foreach (ApplicationButton button in appButtons)
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(@"C:\Windows\system32\taskmgr.exe");
                button.Button.Content = new Image { Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()) };
                button.Button.Width = 30;
                button.Button.Height = 30;
                button.Button.Margin = new Thickness(btnCount * 30, SystemParameters.WorkArea.Height - 100, 0, 0);

                btnCount++;
            }
        }

        private void SetupChronoButtons()
        {
            btnStartStopChrono.Margin = new Thickness(0, SystemParameters.WorkArea.Height - 30, 0, 0);
            btnStartStopChrono.Height = 30;
            btnStartStopChrono.Width = 30;
            btnStartStopChrono.Background = Brushes.Green;

            btnResetChrono.Margin = new Thickness(30, SystemParameters.WorkArea.Height - 30, 0, 0);
            btnResetChrono.Height = 30;
            btnResetChrono.Width = 30;
            btnResetChrono.Background = Brushes.Yellow;

            lblChrono.Margin = new Thickness(60, SystemParameters.WorkArea.Height - 30, 0, 0);
            lblChrono.Height = 30;
            lblChrono.Width = 40;
        }

        private void BtnIisResetFlushTemp_Click(object sender, RoutedEventArgs e)
        {
            btnIisResetFlushTemp.IsEnabled = false;

            Process process = StartProcess("IISReset.exe");
            process.WaitForExit();

            Directory.Delete(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root", true);

            btnIisResetFlushTemp.IsEnabled = true;
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            Width = 100;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            Width = 10;
        }

        private void BtnTaskManager_Click(object sender, RoutedEventArgs e)
        {
            StartProcess("taskmgr.exe");
        }

        private Process StartProcess(string processPath)
        {
            Process process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = processPath;
            process.Start();

            return process;
        }

        private void BtnStartStopChrono_Click(object sender, RoutedEventArgs e)
        {
            if (chronometer.IsRunning)
            {
                chronometer.Stop();
                btnStartStopChrono.Content = "Start";
                btnStartStopChrono.Background = Brushes.Green;
            }
            else
            {
                chronometer.Start();
                btnStartStopChrono.Content = "Stop";
                btnStartStopChrono.Background = Brushes.Red;
            }
        }

        private void BtnResetChrono_Click(object sender, RoutedEventArgs e)
        {
            chronometer.Reset();
            btnStartStopChrono.Content = "Start";
            lblChrono.Content = "0.0";
        }

        public void Notify()
        {
            lblChrono.Content = Math.Round(chronometer.Elapsed.TotalSeconds, 2);
        }
    }
}
