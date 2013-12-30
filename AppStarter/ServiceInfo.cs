using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter
{
    public class ServiceInfo
    {
        public ServiceInfo (string name, string status, string startMode)
        {
            this.name = name;
            this.status = status;
            this.startMode = startMode;
        }

        //TODO: add type, so the type can be set as disable, auto, or manual

        private string startMode;
        private string name;
        private string status;

        public string Name
        {
            get { return name; }
        }

        public ServiceControllerStatus Status
        {
            get
            {
                return (ServiceControllerStatus) Enum.Parse(typeof (ServiceControllerStatus), status, true);
            }
            set
            {
                status = value.ToString();
            }
        }

        public ServiceStartMode StartMode
        {
            get
            {
                return (ServiceStartMode) Enum.Parse(typeof (ServiceStartMode), startMode, true);
            }
            set
            {
                startMode = value.ToString();
            }
        }
    }
}
