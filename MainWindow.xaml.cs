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
        bool autoUpdate_Enabled = false;
        public MainWindow()
        {
            var ts = new ThreadStart(UpdateWorker);
            var backgroundThread = new Thread(ts);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ts = new ThreadStart(CheckStatus);
            var statusThread = new Thread(ts);
            statusThread.IsBackground = true;
            statusThread.Start();
        }

        private void advancedSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 285
            if (this.Height != 600)
            {
                this.Height = 600;
            }
            else { 
                this.Height = 285;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void autoUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Green: #FFB2D530
            //   Red: #FFDC4444
            if (autoUpdate_Enabled)
            {
                autoUpdate.Background = new SolidColorBrush(Color.FromRgb(220, 68, 68));
                autoUpdate.Content = "Auto Update | OFF";
                autoUpdate_Enabled = false;
            }
            else { 
                autoUpdate.Background = new SolidColorBrush(Color.FromRgb(178, 213, 48));
                autoUpdate.Content = "Auto Update | ON";
                autoUpdate_Enabled = true;
            }
        }
        private void CheckStatus()
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
                Arguments = "/C wsl --list --verbose > output.txt"
            };
            process.Start();
            process.WaitForExit();
            string result = File.ReadAllText("output.txt");
            string[] results = File.ReadAllLines("output.txt");

            Dispatcher.Invoke(new Action(() =>
            {
                richTextBox1.Document.Blocks.Clear();
                richTextBox1.Document.Blocks.Add(new Paragraph(new Run(result.Replace(Environment.NewLine, ""))));
            }));
            string status = "Container Status: Not Installed!";
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
                        status = "Container Status: Stopped";
                    }
                    else if (parsedDistro.Contains("Running"))
                    {
                        status = "Container Status: Running";
                    }
                    break;
                }
            }
            Dispatcher.Invoke(new Action(() =>
            {
                statusLabel.Content = status;
            }));
            // C:\ProgramData\Salad\logs
            var directory = new DirectoryInfo(@"C:\ProgramData\Salad\logs");
            using (var stream = new FileStream(directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        logBuffer.Document.Blocks.Clear();
                    }));
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("ProvidedWorkload"))
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                workloadLabel.Content = "Most Recent Workload:    " + line.Split(") - ")[1];
                                runningSinceLabel.Content = "Running Since:    " + line.Split("[INF]")[0];
                            }));
                            Dispatcher.Invoke(new Action(() =>
                            {
                                TextRange rangeOfLine = new TextRange(logBuffer.Document.ContentEnd, logBuffer.Document.ContentEnd);
                                rangeOfLine.Text = line.Split("[INF]")[0] + line.Split(") - ")[1].Split(" - ")[0] + "\n";
                                rangeOfLine.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.GreenYellow);
                            }));
                        }
                        else if (line.Contains("[WRN]"))
                        {
                            //logs = logs + line + "\n";
                            Dispatcher.Invoke(new Action(() =>
                            {
                                if (showWarnings.IsChecked == true)
                                {
                                    TextRange rangeOfLine = new TextRange(logBuffer.Document.ContentEnd, logBuffer.Document.ContentEnd);
                                    rangeOfLine.Text = line + "\n";
                                    rangeOfLine.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Orange);
                                }
                            }));

                        }
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        TextRange textRange = new TextRange(logBuffer.Document.ContentStart, logBuffer.Document.ContentEnd);
                        MemoryStream ms = new MemoryStream();
                        textRange.Save(ms, DataFormats.Rtf);
                        

                        FlowDocument fd = new FlowDocument();
                        MemoryStream ms2 = new MemoryStream(Encoding.ASCII.GetBytes(Encoding.Default.GetString(ms.ToArray())));
                        TextRange textRange2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                        textRange2.Load(ms2, DataFormats.Rtf);


                        workloadLogs.Document.Blocks.Clear();
                        workloadLogs.Document = fd;
                        workloadLogs.ScrollToEnd();
                    }));
                }
            }
        }
        private void UpdateWorker()
        {
            while (true)
            {  
                if (autoUpdate_Enabled)
                {
                    CheckStatus();
                    Thread.Sleep(60000);
                }
                Thread.Sleep(1000);
            }
        }

        private void richTextBox1_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void issuesButton(object sender, RoutedEventArgs e)
        {
            var destinationurl = "https://github.com/pelpadesa/sce-status/issues/";
            var sInfo = new ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            Process.Start(sInfo);
        }

        private void copyLogsButton(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                TextRange textRange = new TextRange(workloadLogs.Document.ContentStart, workloadLogs.Document.ContentEnd);
                Clipboard.SetText(textRange.Text);
            }));
        }
    }
}
