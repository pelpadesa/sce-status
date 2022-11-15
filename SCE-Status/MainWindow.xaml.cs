﻿using System;
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
                Arguments = "/C wsl --list --verbose > output.txt"
            };
            process.Start();
            process.WaitForExit();
            string result = File.ReadAllText("output.txt");
            string[] results = File.ReadAllLines("output.txt");

            richTextBox1.Document.Blocks.Clear();
            richTextBox1.Document.Blocks.Add(new Paragraph(new Run(result.Replace(Environment.NewLine, ""))));
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
                    break;
                }
            }
            if (!set)
            {
                statusLabel.Content = "Container Status: Not Installed!";
            }
            // C:\ProgramData\Salad\logs
            var directory = new DirectoryInfo(@"C:\ProgramData\Salad\logs");
            using (var stream = new FileStream(directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First().FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
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

    }
}
