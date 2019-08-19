using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Taskbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifiable
    {
        private const int WIDTH_OPEN = 100;
        private const int WIDTH_CLOSE = 10;
        private List<ApplicationButton> appButtons = new List<ApplicationButton>();
        private Chronometer chronometer = new Chronometer();
        private const string BASE_ROOT = @"C:\Tfs";

        private string Workspace => cbo_workspace.SelectedItem.ToString();

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

            foreach (var folder in Directory.EnumerateDirectories(BASE_ROOT, "*", SearchOption.TopDirectoryOnly))
                cbo_workspace.Items.Add(folder);

            cbo_workspace.SelectedIndex = 0;

            SetupChronoButtons();
            SetupTimerButtons();
            SetupAppButtons();
        }

        private void SetupAppButtons()
        {
            int btnCount = 1;
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

        private void SetupTimerButtons()
        {
            btnTimer.Margin = new Thickness(0, btnStartStopChrono.Margin.Top - 30, 0, 0);
            btnTimer.Width = 30;
            btnTimer.Height = 30;
            btnTimer.Background = Brushes.LightGreen;

            txtTimer.Margin = new Thickness(30, btnTimer.Margin.Top, 0, 0);
            txtTimer.Height = 30;
            txtTimer.Width = WIDTH_OPEN - 30;
        }

        private void SetupChronoButtons()
        {
            btnStartStopChrono.Margin = new Thickness(0, SystemParameters.WorkArea.Height - 30, 0, 0);
            btnStartStopChrono.Height = 30;
            btnStartStopChrono.Width = 30;
            btnStartStopChrono.Background = Brushes.LightGreen;

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
            btnIisResetFlushTemp.Background = Brushes.Red;

            ThreadHelper.CreateAndStart(() =>
            {
                try
                {
                    Process process = StartProcess("IISReset.exe");
                    process.WaitForExit();

                    Directory.Delete(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root", true);
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnIisResetFlushTemp.ClearValue(BackgroundProperty);
                    });
                }
            });
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            Width = WIDTH_OPEN;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            Width = WIDTH_CLOSE;
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
                btnStartStopChrono.Background = Brushes.LightGreen;
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
            btnStartStopChrono.Background = Brushes.LightGreen;
            lblChrono.Content = "0.0";
        }

        public void Notify()
        {
            lblChrono.Content = Math.Round(chronometer.Elapsed.TotalSeconds, 2);
        }

        private void BtnFlushBinDll_Click(object sender, RoutedEventArgs e)
        {
            btnFlushBinDll.Background = Brushes.Red;

            try
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(Workspace, @"Legacy\OFSYS\WebSite\Bin"), "*.dll", SearchOption.AllDirectories))
                {
                    if (file.ToLower().Contains("roslyn"))
                        continue;

                    File.Delete(file);
                }
            }
            finally
            {
                Dispatcher.Invoke(() => btnFlushBinDll.ClearValue(BackgroundProperty));
            }
        }

        private void TxtTimer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key < Key.D0 || e.Key > Key.NumPad9)
            {
                e.Handled = true;
                return;
            }

            if (e.Key > Key.D9 && e.Key < Key.NumPad0)
            {
                e.Handled = true;
                return;
            }
        }

        private void btnFlushBinObjFolders_Click(object sender, RoutedEventArgs e)
        {
            btnFlushBinDll.Background = Brushes.Red;

            try
            {
                foreach (var directory in Directory.EnumerateDirectories(Workspace, "*", SearchOption.AllDirectories).Where(d => !d.ToLower().Contains("website") && (d.ToLower().EndsWith(@"\bin") || d.ToLower().EndsWith(@"\obj"))))
                {
                    foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                        File.Delete(file);
                }
            }
            finally
            {
                Dispatcher.Invoke(() => btnFlushBinDll.ClearValue(BackgroundProperty));
            }
        }
    }
}
