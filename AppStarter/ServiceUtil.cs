using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace AppStarter
{
    public delegate void ServiceEventHandler(object sender, EventArgs e);

    public class ServiceUtil
    {
        public const uint DEFAULT_TIME_OUT = 20;

        public ServiceUtil(uint timeout, List<ServiceInfo> info)
        {
            this.timeout = timeout;
            this.servicesInfo = info;

            serviceController=new ServiceController();
        }

        /// <summary>
        /// Dispatched when the services processing is progressed.
        /// </summary>
        public event ServiceEventHandler Progress;

        /// <summary>
        /// Dispatched when the service operation is completed.
        /// </summary>
        public event ServiceEventHandler Completed;

        /// <summary>
        /// Dispatched when the service operation cannot be completed or timeout.
        /// </summary>
        public event ServiceEventHandler Error;

        /// <summary>
        /// In seconds.
        /// </summary>
        private uint timeout = DEFAULT_TIME_OUT;

        private List<ServiceInfo> servicesInfo;

        private bool processing = false;

        private ServiceController serviceController;

        public void ProcessServices()
        {
            if (this.servicesInfo != null)
            {
                processing = true;

                processIdx = 0;

                processServices();
            }
        }

        private int processIdx;

        private void processServices()
        {
            if (processIdx < servicesInfo.Count())
            {
                this.Progress += new ServiceEventHandler(progressHandler);

                ServiceInfo serviceInfo = this.servicesInfo[processIdx];

                switch (serviceInfo.ServiceStatus)
                {
                    case ServiceControllerStatus.Running:
                        startService(serviceInfo.Name);
                        break;
                    //TODO: other status
                }
            }
        }

        private void startService(string serviceName)
        {
            serviceController.ServiceName = serviceName;

            if (serviceController.Status.Equals(ServiceControllerStatus.Running)) //already running
            {
                OnProgress(processIdx + 1);
                return;
            }

            try
            {
                TimeSpan timeoutSec = TimeSpan.FromSeconds(timeout);

                serviceController.Start(); //TODO: support arguments
                serviceController.WaitForStatus(ServiceControllerStatus.Running, timeoutSec);

                //check the status to make sure it is started properly
                if (serviceController.Status.Equals(ServiceControllerStatus.Running))
                {
                    OnProgress(processIdx + 1);
                }
                else
                {
                    throw new InvalidOperationException(serviceName + ": cannot be started");
                }
            }
            //TODO: there should be configuration for the app
            //when error occurs to a service, should it stop or continue to next service?
            catch (InvalidOperationException e)
            {
                OnError();
            }
            catch (System.ServiceProcess.TimeoutException e)
            {
                OnError();
            }
        }

        private void progressHandler(object sender, EventArgs e)
        {
            this.Progress -= progressHandler;

            //TODO: deal with error, when error occurs, maybe skip the service and go to next

            processIdx++;
            if (processIdx < this.servicesInfo.Count())
                processServices();
            else
                OnComplete();
        }

        protected void OnError()
        {
            if (Error != null)
                Error(this, EventArgs.Empty);
        }

        protected void OnProgress(int idx)
        {
            if (Progress!=null)
            {
                ServiceProcessProgressEventArgs arg = new ServiceProcessProgressEventArgs();
                arg.NumCompleted = idx;
                arg.Total = this.servicesInfo.Count();
                Progress(this, arg);
            }
        }

        protected void OnComplete()
        {
            processing = false;

            serviceController.Close();

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }
    } //class end

    public class ServiceProcessProgressEventArgs:EventArgs
    {
        public int NumCompleted { get; set; }
        public int Total { get; set; }
    }
}
