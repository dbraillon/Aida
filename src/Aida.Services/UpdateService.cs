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
            GRoggle.Write("Try starting Aida process", RoggleLogLevel.Debug);

            TryAttachProcessIfNecessary();

            if (ApplicationProcess != null)
            {
                if (ApplicationProcess.)
                {
                    ApplicationProcess.Start();

                    GRoggle.Write("Aida process started successfuly", RoggleLogLevel.Debug);
                }
                else
                {
                    GRoggle.Write("There is already an Aida process started", RoggleLogLevel.Debug);
                }
            }
            else
            {
                GRoggle.Write("There is no Aida process to start", RoggleLogLevel.Debug);
            }
        }

        /// <summary>
        /// Stop main application process if not already stopped.
        /// </summary>
        protected void StopApplicationProcess()
        {
            GRoggle.Write("Try stopping Aida process", RoggleLogLevel.Debug);

            TryAttachProcessIfNecessary();

            if (ApplicationProcess != null)
            {
                if (!ApplicationProcess.HasExited)
                {
                    StopProcess(ApplicationProcess);

                    GRoggle.Write("Aida process stopped successfuly", RoggleLogLevel.Debug);
                }
                else
                {
                    GRoggle.Write("Aida process has already been stopped", RoggleLogLevel.Debug);
                }
            }
            else
            {
                GRoggle.Write("There is no Aida process to stop", RoggleLogLevel.Debug);
            }
        }

        /// <summary>
        /// Try to attach an Aida process to current process.
        /// </summary>
        protected void TryAttachProcessIfNecessary()
        {
            GRoggle.Write("Try attaching an existing Aida process", RoggleLogLevel.Debug);

            if (ApplicationProcess == null)
            {
                // Get all Aida processes
                var processes = Process.GetProcessesByName(ApplicationProcessName);

                GRoggle.Write($"There is {processes.Length} Aida processes running", RoggleLogLevel.Debug);

                if (processes.Length > 0)
                {
                    // If there is at least 1 process running, attach it
                    ApplicationProcess = processes.First();
                    GRoggle.Write($"Process with ID {ApplicationProcess.Id} has been attached", RoggleLogLevel.Debug);

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

                    GRoggle.Write("There is no Aida process running, create one", RoggleLogLevel.Debug);
                }
            }
            else
            {
                GRoggle.Write("There is already an Application process attached", RoggleLogLevel.Debug);
            }
        }

        /// <summary>
        /// Try to stop all other Aida instance.
        /// </summary>
        protected void StopConcurrentProcess()
        {
            GRoggle.Write("Try to stop concurrent Aida process", RoggleLogLevel.Debug);

            // Get all Aida processes
            var processes = Process.GetProcessesByName(ApplicationProcessName);
            
            GRoggle.Write($"There are {processes.Length} Aida process running", RoggleLogLevel.Debug);

            // Loop on each
            foreach (var process in processes)
            {
                // Do not stop current process
                if ((ApplicationProcess != null && process.Id != ApplicationProcess.Id) ||
                    ApplicationProcess == null)
                {
                    GRoggle.Write($"Stop process with ID {process.Id}", RoggleLogLevel.Debug);

                    StopProcess(process);
                }
                else
                {
                    GRoggle.Write($"Do not stop current Aida process {process.Id}", RoggleLogLevel.Debug);
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
                GRoggle.Write("Try stopping Aida process", RoggleLogLevel.Debug);

                // Tell Aida to stop kindly
                process.StandardInput.WriteLine(ApplicationExitCommand);

                // Wait defined time then try the hard way
                if (process.WaitForExit(ApplicationExitWaitTime))
                {
                    GRoggle.Write("Aida stopped kindly", RoggleLogLevel.Debug);
                }
                else
                {
                    throw new ApplicationException($"Process does not stop in specified time ({ApplicationExitWaitTime} ms).");
                }
            }
            catch (InvalidOperationException e)
            {
                GRoggle.Write("Something goes wrong while stopping Aida process", e);
            }
            catch (ApplicationException e)
            {
                GRoggle.Write(e);
            }
            finally
            {
                // Finally, if nothing has worked, try the hard way
                if (!process.HasExited)
                {
                    GRoggle.Write("Try killing Aida process", RoggleLogLevel.Debug);
                    process.Kill();
                }
                else
                {
                    GRoggle.Write("Nothing to do, Aida stopped kindly", RoggleLogLevel.Debug);
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
                GRoggle.Write("Try getting last release version", RoggleLogLevel.Debug);

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
                GRoggle.Write("Something goes wrong while getting last release version", e);

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
                GRoggle.Write($"Try downloading last release {release.Name}", RoggleLogLevel.Debug);

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
                        GRoggle.Write("Can't find any 'release' asset in GitHub releases", RoggleLogLevel.Debug);
                    }
                }
            }
            catch (WebException e)
            {
                GRoggle.Write("Something goes wrong while downloading last release", e);
            }
            catch (IOException e)
            {
                GRoggle.Write("Something goes wrong while downloading last release", e);
            }
            catch (UnauthorizedAccessException e)
            {
                GRoggle.Write("Something goes wrong while downloading last release", e);
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
                GRoggle.Write("Try to clear application directory", RoggleLogLevel.Debug);

                foreach (var file in ApplicationDirectory.EnumerateFiles())
                {
                    if (file.Name != ApplicationReleaseFileName)
                    {
                        GRoggle.Write($"Delete {file.Name}", RoggleLogLevel.Debug);

                        file.Delete();
                    }
                }
            }
            catch (IOException e)
            {
                GRoggle.Write("Something goes wrong while clearing application directory", e);
            }
            catch (UnauthorizedAccessException e)
            {
                GRoggle.Write("Something goes wrong while clearing application directory", e);
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
                GRoggle.Write("Try to extract a release zip", RoggleLogLevel.Debug);

                ZipFile.ExtractToDirectory(releaseFile.FullName, ApplicationDirectory.FullName);
                File.Delete(releaseFile.FullName);
            }
            catch (IOException e)
            {
                GRoggle.Write("Something goes wrong while extracting release", e);
            }
            catch (UnauthorizedAccessException e)
            {
                GRoggle.Write("Something goes wrong while extracting release", e);
            }
        }
    }
}
