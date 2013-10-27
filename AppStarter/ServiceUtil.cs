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

        public ServiceUtil(uint timeout)
        {
            this.timeout = timeout;
        }

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

        public void startService(string serviceName)
        {
            try
            {
                TimeSpan timeoutSec = TimeSpan.FromSeconds(timeout);

                ServiceController service = new ServiceController(serviceName);
                service.Start(); //TODO: support arguments
                service.WaitForStatus(ServiceControllerStatus.Running, timeoutSec);

                OnComplete();
            }
            catch (System.ServiceProcess.TimeoutException e)
            {
                OnError();
            }
        }

        protected void OnError()
        {
            if (Error != null)
                Error(this, EventArgs.Empty);
        }

        protected void OnComplete()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }
    } //class end
}
