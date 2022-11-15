using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
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

namespace SCE_Status
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                FileName = "cmd.exe",
                Arguments = "/C wsl --list --verbose"
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] results = result.Split("\n");

            richTextBox1.Document.Blocks.Clear();
            richTextBox1.Document.Blocks.Add(new Paragraph(new Run(result.Replace("\n", ""))));
            bool set = false;
            foreach (string distro in results)
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in distro)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '-')
                    {
                        sb.Append(c);
                    }
                }
                string parsedDistro = sb.ToString();
                if (parsedDistro.Contains("salad-enterprise-linux"))
                {
                    if (parsedDistro.Contains("Stopped"))
                    {
                        statusLabel.Content = "Container Status: Stopped";
                    }
                    else if (parsedDistro.Contains("Running"))
                    {
                        statusLabel.Content = "Container Status: Running";
                    }
                    set = true;
                }
                if (!set)
                {
                    statusLabel.Content = "Container Status: Not Installed!";
                }
            }
            // C:\ProgramData\Salad\logs
            var directory = new DirectoryInfo(@"C:\ProgramData\Salad\logs");
            var logFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            string[] lines = File.ReadAllLines(logFile.FullName);
            foreach (string line in lines)
            {
                if (line.Contains("ProvidedWorkload"))
                {
                    workloadLabel.Content = "Most Recent Workload:    " + line.Split(") - ")[1];
                    runningSinceLabel.Content = "Running Since:    " + line.Split("[INF]")[0];
                }
            }

        }

    }
}
