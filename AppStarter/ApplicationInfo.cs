using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AppStarter
{
    public class ApplicationInfo
    {
        public ApplicationInfo(XDocument xml)
        {
            //XElement nameElement = xml.Root.Element("name");
            
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
        }

        private ProcessStartInfo mainProgramStartInfo;
        private Process mainProgram;

        private ServiceUtil serviceUtil;

        public void Start()
        {
            serviceUtil.ProcessServices();
        }

        void serviceUtil_Error(object sender, EventArgs e)
        {
            Console.WriteLine("Error");
        }
        
        void serviceUtil_Completed(object sender, EventArgs e)
        {
            serviceUtil.Completed -= serviceUtil_Completed;
            serviceUtil.Completed += serviceUtil_End;
            //TODO: attach another complete handler, so that other actions after all services are stopped

            //start the main program
            mainProgram = new Process();
            mainProgram.StartInfo = mainProgramStartInfo;
            mainProgram.Start();
            mainProgram.WaitForExit();

            //when the main program stops, stop the services and reset the start mode to disabled.
            List<ServiceInfo> servicesInfo = serviceUtil.ServicesInfo;
            foreach (ServiceInfo serviceInfo in servicesInfo)
            {
                if (serviceInfo.Status.Equals(ServiceControllerStatus.Running))
                {
                    serviceInfo.Status = ServiceControllerStatus.Stopped;
                }
            }
            servicesInfo.Reverse();

            serviceUtil.ServicesInfo = servicesInfo;
            Start();
        }

        void serviceUtil_End(object sender, EventArgs e)
        {
            Console.WriteLine("restored the status");
        }
    } //class end
}
