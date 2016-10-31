using Aida.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aida.WindowsServices
{
    public partial class UpdateService : ServiceBase
    {
        protected DirectoryInfo ApplicationDirectory { get; set; }
        protected Thread ServiceThread { get; set; }
        protected volatile bool IsRunning;

        public UpdateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ApplicationDirectory = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Application"));
            IsRunning = true;
            ServiceThread = new Thread(Loop);
            ServiceThread.Start();
        }

        protected override void OnStop()
        {
            IsRunning = false;
            ServiceThread.Join();
        }

        protected void Loop()
        {
            while (IsRunning)
            {
                var url = "https://github.com/dbraillon/Aida/releases/download/last-version/last.zip";
                var zipFile = Path.Combine(ApplicationDirectory.FullName, "last.zip");

                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(url, zipFile);
                }

                // Wait one day
                Thread.Sleep(1000 * 60 * 60 * 24);
            }
        }
    }
}
