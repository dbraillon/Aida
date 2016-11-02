using Aida.WindowsServices.GitHub;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace Aida.WindowsServices
{
    public partial class UpdateService : ServiceBase
    {
        protected DirectoryInfo ApplicationDirectory { get; set; }
        protected Process ApplicationProcess { get; set; }

        protected Thread ServiceThread { get; set; }
        protected volatile bool IsRunning;

        public UpdateService()
        {
            InitializeComponent();
        }

        public void DebugStart()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            ApplicationDirectory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "Aida"));
            ApplicationProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    FileName = Path.Combine(ApplicationDirectory.FullName, "Aida.exe"),
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };

            IsRunning = true;
            ServiceThread = new Thread(Loop);
            ServiceThread.Start();
        }

        protected override void OnStop()
        {
            ApplicationProcess?.StandardInput.WriteLine("exit");
            ApplicationProcess?.WaitForExit();
            IsRunning = false;
            ServiceThread.Interrupt();
            ServiceThread.Join();
        }

        protected void Loop()
        {
            var applicationFilePath = Path.Combine(ApplicationDirectory.FullName, "Aida.exe");
            if (File.Exists(applicationFilePath)) ApplicationProcess.Start();

            while (IsRunning)
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    var releasesUrl = "https://api.github.com/repos/dbraillon/aida/releases";
                    var releasesJsonStr = webClient.DownloadString(releasesUrl);
                    var releasesJson = JsonConvert.DeserializeObject<IEnumerable<Release>>(releasesJsonStr);
                    var releasesVersions = releasesJson.OrderByDescending(r => r.Name, new VersionComparer());
                    var lastVersion = releasesVersions.First();

                    var currentVersion = File.Exists(applicationFilePath) ? FileVersionInfo.GetVersionInfo(applicationFilePath).ProductVersion : "0.0.0.0";

                    if (lastVersion.CompareTo(currentVersion) > 0)
                    {
                        var url = lastVersion.Assets.First(a => a.Name == "release.zip").BrowserDownloadUrl;
                        var zipFile = Path.Combine(ApplicationDirectory.FullName, "release.zip");

                        if (File.Exists(zipFile)) File.Delete(zipFile);
                        webClient.DownloadFile(url, zipFile);

                        ApplicationProcess?.StandardInput.WriteLine("exit");
                        ApplicationProcess?.WaitForExit();

                        //Directory.Delete(ApplicationDirectory.FullName, true);
                        //Directory.CreateDirectory(ApplicationDirectory.FullName);
                        foreach (var file in Directory.EnumerateFiles(ApplicationDirectory.FullName))
                            if (Path.GetFileName(file) != "release.zip")
                                File.Delete(file);
                        ZipFile.ExtractToDirectory(zipFile, ApplicationDirectory.FullName);
                        File.Delete(zipFile);
                    }

                    if (File.Exists(applicationFilePath)) ApplicationProcess.Start();
                }

                // Wait one hour
                Thread.Sleep(1000 * 60 * 60);
            }
        }
    }
}
