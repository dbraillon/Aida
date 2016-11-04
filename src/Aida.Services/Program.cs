using Roggle.Core;
using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace Aida.Services
{
    static class Program
    {
        /// <summary>
        /// Contains all services this program defines.
        /// </summary>
        static ServiceBase[] Services
        {
            get
            {
                return new ServiceBase[]
                {
                    new UpdateService()
                };
            }
        }

        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.FirstOrDefault() == "-i") Install();
                if (args.FirstOrDefault() == "-u") Uninstall();
                if (args.FirstOrDefault() == "-d") Debug();
            }
            else
            {
                ServiceBase.Run(Services);
            }
        }

        static void Install()
        {
            // Create Roggle log
            GRoggle.Use(
                new EventLogRoggle(
                    eventSourceName: "Aida.Services", eventLogName: "Aida", 
                    acceptedLogLevels: RoggleLogLevel.Debug | RoggleLogLevel.Error | RoggleLogLevel.Info | RoggleLogLevel.Warning
                )
            );

            // Start installutil.exe to install services
            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

            // Start services if necessary
            foreach (var service in Services)
            {
                ServiceController serviceController = new ServiceController(service.ServiceName);
                serviceController.Start();
            }
        }

        static void Uninstall()
        {
            // Stop services if necessary
            foreach (var service in Services)
            {
                ServiceController serviceController = new ServiceController(service.ServiceName);
                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    serviceController.Stop();
                }
            }
            
            // Start installutil.exe to uninstall services
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }

        static void Debug()
        {
            // Start services in debug mode
            new UpdateService().DebugStart();
        }
    }
}
