using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Xml.Linq;

namespace AppStarter
{
    public delegate void ApplicationInfoEventHandler(object sender, EventArgs e);

    public delegate bool ServiceCheckFunction(ApplicationInfo sender, string serviceName);

    public class ApplicationInfo
    {
        public ApplicationInfo(XDocument xml)
        {
            XElement nameElement = xml.Root.Element("name");
            applicationName = nameElement.Value.Trim();

            XElement pathElement = xml.Root.Element("path");
            mainProgramStartInfo = new ProcessStartInfo(pathElement.Value.Trim());

            var services = from svrs in xml.Root.Elements("services").Elements("service")
                           select new
                           {
                               name = svrs.Element("name").Value,
                               status = svrs.Element("status").Value
                           };

            

            List<ServiceInfo> servicesInfo = new List<ServiceInfo>();
            foreach (var one in services)
            {
                Console.WriteLine(one.name + ":" + one.status);
                servicesInfo.Add(new ServiceInfo(one.name.Trim(), one.status.Trim(), ""));
            }

            serviceUtil = new ServiceUtil(ServiceUtil.DEFAULT_TIME_OUT, servicesInfo);
            serviceUtil.Completed += serviceUtil_Completed;
            serviceUtil.Error += serviceUtil_Error;

            hasMainProgramExited = false;
        }

        public event ApplicationInfoEventHandler Completed;
        public ServiceCheckFunction ServiceCheckCallback;

        private ProcessStartInfo mainProgramStartInfo;
        private Process mainProgram;
        private bool hasMainProgramExited;

        private ServiceUtil serviceUtil;

        private string applicationName;
        public string ApplicationName
        {
            get
            {
                return applicationName;
            }
        }

        public void Start()
        {
            hasMainProgramExited = false;

            BackgroundWorker appThread=new BackgroundWorker();
            appThread.WorkerReportsProgress = false;
            appThread.WorkerSupportsCancellation = false;

            appThread.DoWork += appThread_DoWork;
            appThread.RunWorkerCompleted += appThread_RunWorkerCompleted;
            appThread.RunWorkerAsync();
        }

        public bool IsServiceUsed(string serviceName)
        {
            if (hasMainProgramExited) //any service required for the app is no more needed
                return false;
            else
                return serviceUtil.ServicesInfo.FindIndex(info => info.Name == serviceName) >= 0;
        }

        void appThread_DoWork(object sender, DoWorkEventArgs e)
        {
            serviceUtil.ProcessServices();
        }

        void appThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        void serviceUtil_Error(object sender, EventArgs e)
        {
            Console.WriteLine("Error");
        }
        
        void serviceUtil_Completed(object sender, EventArgs e)
        {
            serviceUtil.Completed -= serviceUtil_Completed;
            serviceUtil.Completed += serviceUtil_End;

            //start the main program
            mainProgram = new Process();
            mainProgram.StartInfo = mainProgramStartInfo;
            mainProgram.Start();
            mainProgram.WaitForExit();

            hasMainProgramExited = true;

            //after the main program stops
            List<ServiceInfo> servicesInfo = serviceUtil.ServicesInfo;

            //if the service is used by other app, untouch the service, so remove from the list
            servicesInfo.RemoveAll(svcInfo => ServiceCheckCallback(this, svcInfo.Name));
            //stop the services and reset the start mode to disabled
            foreach (ServiceInfo serviceInfo in servicesInfo)
            {
                if (serviceInfo.Status.Equals(ServiceControllerStatus.Running))
                {
                    serviceInfo.Status = ServiceControllerStatus.Stopped;
                }
            }
            servicesInfo.Reverse(); //some services might depend on each other, so reverse the order of starting

            serviceUtil.ServicesInfo = servicesInfo;
            serviceUtil.ProcessServices();
        }

        void serviceUtil_End(object sender, EventArgs e)
        {
            serviceUtil.Completed -= serviceUtil_End;

            mainProgram.Close();
            mainProgram.Dispose();
        }
    } //class end
}
