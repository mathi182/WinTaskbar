using System;
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

namespace Taskbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ApplicationButton> AppButtons = new List<ApplicationButton>();

        public MainWindow()
        {
            InitializeComponent();

            AppButtons.Add(new ApplicationButton { Button = btnTaskManager, IconPath = @"C:\Windows\system32\taskmgr.exe" });

            Background = Brushes.Gray;
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
            Left = 0;

            SetupAppButtons();
        }

        private void SetupAppButtons()
        {
            int btnCount = 0;
            foreach (ApplicationButton button in AppButtons)
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(@"C:\Windows\system32\taskmgr.exe");
                button.Button.Content = new Image { Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()) };
                button.Button.Width = 30;
                button.Button.Height = 30;
                button.Button.Margin = new Thickness(btnCount * 30, SystemParameters.WorkArea.Height - 30, 0, 0);

                btnCount++;
            }
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
    }
}
