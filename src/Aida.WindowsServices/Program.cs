using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace Aida.WindowsServices
{
    static class Program
    {
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
            }
            else
            {
                ServiceBase.Run(Services);
            }
        }

        static void Install()
        {
            // Start installutil.exe to install services
            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

            // Start services if necessary
            foreach (var service in Services)
            {
                ServiceController serviceController = new ServiceController(new UpdateService().ServiceName);
                serviceController.Start();
            }
        }

        static void Uninstall()
        {
            // Stop services if necessary
            foreach (var service in Services)
            {
                ServiceController serviceController = new ServiceController(new UpdateService().ServiceName);
                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    serviceController.Stop();
                }
            }
            
            // Start installutil.exe to uninstall services
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }
    }
}
