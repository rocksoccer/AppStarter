using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppStarter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            Console.WriteLine("hello world");

            //TODO: read the config from file
            List<ServiceInfo> info=new List<ServiceInfo>();
            //info.Add(new ServiceInfo("VMAuthdService", "", "running"));
            //info.Add(new ServiceInfo("VMUSBArbService", "", "running"));
            info.Add(new ServiceInfo("VMwareHostd", "", "running"));

            ServiceUtil serviceUtil = new ServiceUtil(ServiceUtil.DEFAULT_TIME_OUT);
            processServices(serviceUtil, info, 0);
        }

        private void processServices(ServiceUtil serviceUtil, List<ServiceInfo> info, int idx)
        {
            serviceUtil.Completed += (sender, eventArgs) =>
            {
                idx++;
                if(idx<info.Count())
                    processServices(serviceUtil, info, idx);
                else
                {
                    Console.WriteLine("finished");
                }
            };

            //TODO: deal with error, when error occurs, maybe skip the service and go to next

            switch (info[idx].ServiceStatus)
            {
                case ServiceControllerStatus.Running:
                    serviceUtil.startService(info[idx].Name);
                    break;
                //TODO: other status
            }
        }
    } //class end
}
