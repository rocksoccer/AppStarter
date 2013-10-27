using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter
{
    class ServiceInfo
    {
        public ServiceInfo (string name, string type, string status)
        {
            this.name = name;
            this.status = status;
        }

        //TODO: add type, so the type can be set as disable, auto, or manual

        private string name;
        private string status;

        public string Name
        {
            get { return name; }
        }

        public ServiceControllerStatus ServiceStatus
        {
            get
            {
                return (ServiceControllerStatus) Enum.Parse(typeof (ServiceControllerStatus), status, true);
            }
        }
    }
}
