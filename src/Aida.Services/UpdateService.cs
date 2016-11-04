using Aida.Services.GitHub;
using Newtonsoft.Json;
using Roggle.Core;
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

namespace Aida.Services
{
    public partial class UpdateService : ServiceBase
    {
        public const string ApplicationProcessName = "Aida";
        public const string ApplicationFileName = "Aida.exe";
        public const string ApplicationExitCommand = "exit";
        public const int ApplicationExitWaitTime = 30000;
        public const string ApplicationCheckUpdateUrl = "https://api.github.com/repos/dbraillon/aida/releases";
        public const string ApplicationReleaseFileName = "release.zip";

        /// <summary>
        /// Directory where the main application is stored.
        /// </summary>
        protected DirectoryInfo ApplicationDirectory { get; set; }

        /// <summary>
        /// Process of the main application.
        /// </summary>
        protected Process ApplicationProcess { get; set; }

        /// <summary>
        /// Service thread.
        /// </summary>
        protected Thread ServiceThread { get; set; }

        /// <summary>
        /// Flag to get/set the running state of the service.
        /// </summary>
        protected volatile bool IsRunning;

        public UpdateService()
        {
            InitializeComponent();

            // Get application directory
            ApplicationDirectory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "Aida"));

            // Create service thread
            ServiceThread = new Thread(Loop);
        }

        /// <summary>
        /// Called when service is started in debug mode.
        /// </summary>
        public void DebugStart()
        {
            OnStart(null);
        }

        /// <summary>
        /// Called when service starts.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // Start main application
            StartApplicationProcess();
            
            // Start service
            IsRunning = true;
            ServiceThread.Start();
        }

        /// <summary>
        /// Called when service stops.
        /// </summary>
        protected override void OnStop()
        {
            // Stop main application
            StopApplicationProcess();

            // Stop service
            IsRunning = false;
            ServiceThread.Interrupt();
            ServiceThread.Join();
        }

        /// <summary>
        /// Service thread loop.
        /// </summary>
        protected void Loop()
        {
            while (IsRunning)
            {
                try
                {
                    GRoggle.Write("Start checking if Aida is up to date", RoggleLogLevel.Debug);

                    var applicationFilePath = Path.Combine(ApplicationDirectory.FullName, ApplicationFileName);

                    var lastReleaseVersion = GetLastReleaseVersion();
                    var currentVersion = File.Exists(applicationFilePath) ? FileVersionInfo.GetVersionInfo(applicationFilePath).ProductVersion : "0.0.0.0";

                    GRoggle.Write($"Aida version is {currentVersion}, server version is {lastReleaseVersion.Name}", RoggleLogLevel.Debug);

                    if (lastReleaseVersion.CompareTo(currentVersion) > 0)
                    {
                        GRoggle.Write("Aida needs an update, downloading last version", RoggleLogLevel.Debug);

                        var releaseZipFile = DownloadRelease(lastReleaseVersion);

                        if (releaseZipFile != null)
                        {
                            GRoggle.Write("Download last version succeed, updating", RoggleLogLevel.Debug);

                            StopApplicationProcess();

                            ClearApplicationDirectory();
                            ExtractRelease(releaseZipFile);

                            StartApplicationProcess();

                            GRoggle.Write("Aida has successfuly been updated", RoggleLogLevel.Debug);
                        }
                        else
                        {
                            GRoggle.Write("Download last version failed, skip update", RoggleLogLevel.Debug);
                        }
                    }
                    else
                    {
                        GRoggle.Write("Aida is up to date, skip update", RoggleLogLevel.Debug);
                    }
                }
                catch (Exception e)
                {
                    GRoggle.Write("An unhandled error occurs", e);
                }
                
                // Wait one hour
                Thread.Sleep(1000 * 60 * 60);
            }
        }

        /// <summary>
        /// Start main application process if not already started.
        /// </summary>
        protected void StartApplicationProcess()
        {
            TryAttachProcessIfNecessary();

            if (ApplicationProcess != null)
            {
                if (ApplicationProcess.HasExited)
                {
                    ApplicationProcess.Start();
                }
            }
            else
            {
                // TODO: Log
            }
        }

        /// <summary>
        /// Stop main application process if not already stopped.
        /// </summary>
        protected void StopApplicationProcess()
        {
            TryAttachProcessIfNecessary();

            if (ApplicationProcess != null)
            {
                if (!ApplicationProcess.HasExited)
                {
                    StopProcess(ApplicationProcess);
                }
                else
                {
                    // TODO: Log
                }
            }
            else
            {
                // TODO: Log
            }
        }

        /// <summary>
        /// Try to attach an Aida process to current process.
        /// </summary>
        protected void TryAttachProcessIfNecessary()
        {
            if (ApplicationProcess == null)
            {
                // Get all Aida processes
                var processes = Process.GetProcessesByName(ApplicationProcessName);

                if (processes.Length > 0)
                {
                    // If there is at least 1 process running, attach it
                    ApplicationProcess = processes.First();

                    // And kill the other
                    StopConcurrentProcess();
                }
                else
                {
                    // If there is no process running, build the new process
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
                }
            }
            else
            {
                // TODO: Log
            }
        }

        /// <summary>
        /// Try to stop all other Aida instance.
        /// </summary>
        protected void StopConcurrentProcess()
        {
            // Get all Aida processes
            var processes = Process.GetProcessesByName(ApplicationProcessName);

            // Loop on each
            foreach (var process in processes)
            {
                // Do not stop current process
                if ((ApplicationProcess != null && process.Id != ApplicationProcess.Id) ||
                    ApplicationProcess == null)
                {
                    StopProcess(process);
                }
                else
                {
                    // TODO: Log
                }
            }
        }

        /// <summary>
        /// Try to stop a process kindly. Then, if it does not work, try the hard way.
        /// </summary>
        /// <param name="process">A process to stop.</param>
        protected void StopProcess(Process process)
        {
            try
            {
                // Tell Aida to stop kindly
                process.StandardInput.WriteLine(ApplicationExitCommand);

                // Wait defined time then try the hard way
                if (process.WaitForExit(ApplicationExitWaitTime))
                {
                    // TODO: Log
                }
                else
                {
                    throw new ApplicationException($"Process does not stop in specified time ({ApplicationExitWaitTime} ms).");
                }
            }
            catch (InvalidOperationException e)
            {
                // TODO: Log
            }
            catch (ApplicationException e)
            {
                // TODO: Log
            }
            finally
            {
                // Finally, if nothing has worked, try the hard way
                if (!process.HasExited)
                {
                    process.Kill();
                }
                else
                {
                    // TODO: Log
                }
            }
        }

        /// <summary>
        /// Get last version from GitHub releases.
        /// </summary>
        /// <returns>The last version of main application.</returns>
        protected Release GetLastReleaseVersion()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    // Make the request and get the response
                    var releasesJsonStr = webClient.DownloadString(ApplicationCheckUpdateUrl);

                    // Serialize the response to collection of Release objects
                    var releasesJson = JsonConvert.DeserializeObject<IEnumerable<Release>>(releasesJsonStr);

                    // Order version descending
                    var releasesVersions = releasesJson.OrderByDescending(r => r.Name, new VersionComparer());

                    // Get first version
                    return releasesVersions.First();
                }
            }
            catch (WebException e)
            {
                // TODO: Log

                return Release.Default;
            }
        }

        /// <summary>
        /// Download last version from GitHub releases.
        /// </summary>
        /// <param name="release">A release to download.</param>
        /// <param name="filePath">A path where to store downloaded zip file.</param>
        protected FileInfo DownloadRelease(Release release)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    // Try to find the release zip file
                    var releaseAsset = release.Assets.FirstOrDefault(a => a.Name.ToLower() == ApplicationReleaseFileName.ToLower());
                    if (releaseAsset != null)
                    {
                        var url = releaseAsset.BrowserDownloadUrl;
                        var zipFilePath = Path.Combine(ApplicationDirectory.FullName, ApplicationReleaseFileName);

                        // Delete the file if it already exists
                        if (File.Exists(zipFilePath)) File.Delete(zipFilePath);

                        // Download the release zip file
                        webClient.DownloadFile(url, zipFilePath);

                        return new FileInfo(zipFilePath);
                    }
                    else
                    {
                        // TODO: Log
                    }
                }
            }
            catch (WebException e)
            {
                // TODO: Log
            }
            catch (IOException e)
            {
                // TODO: Log
            }
            catch (UnauthorizedAccessException e)
            {
                // TODO: Log
            }

            return null;
        }

        /// <summary>
        /// Remove every file inside application directory.
        /// </summary>
        protected void ClearApplicationDirectory()
        {
            try
            {
                foreach (var file in ApplicationDirectory.EnumerateFiles())
                {
                    if (file.Name != ApplicationReleaseFileName)
                    {
                        file.Delete();
                    }
                }
            }
            catch (IOException e)
            {
                // TODO: Log
            }
            catch (UnauthorizedAccessException e)
            {
                // TODO: Log
            }
        }

        /// <summary>
        /// Extract zip release file to application directory.
        /// </summary>
        /// <param name="releaseFile">A zip release file.</param>
        protected void ExtractRelease(FileInfo releaseFile)
        {
            try
            {
                ZipFile.ExtractToDirectory(releaseFile.FullName, ApplicationDirectory.FullName);
                File.Delete(releaseFile.FullName);
            }
            catch (IOException e)
            {
                // TODO: Log
            }
            catch (UnauthorizedAccessException e)
            {
                // TODO: Log
            }
        }
    }
}
