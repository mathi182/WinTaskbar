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
        public MainWindow()
        {
            InitializeComponent();

            Background = Brushes.Gray;
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
            Left = 0;
        }

        private void BtnIisResetFlushTemp_Click(object sender, RoutedEventArgs e)
        {
            btnIisResetFlushTemp.IsEnabled = false;

            Process process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "IISReset.exe";
            process.Start();

            process.WaitForExit();

            Directory.Delete(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\root", true);

            btnIisResetFlushTemp.IsEnabled = true;
        }
    }
}
