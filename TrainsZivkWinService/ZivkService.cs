using System;
using System.Text;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration.Install;
using System.ComponentModel;

namespace TrainsZivkWinService
{
    public class ZivkService : ServiceBase
    {
        public ServiceHost serviceHost = null;

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the CalculatorService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(Service));

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }

    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "ZIVKWindowsService";
            service.Description = "Сервер получения событий по поездом из ЦИВК";
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
