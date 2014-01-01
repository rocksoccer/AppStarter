using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace AppStarter
{
    public delegate void ServiceEventHandler(object sender, EventArgs e);

    public class ServiceUtil
    {
        public const uint DEFAULT_TIME_OUT = 20;

        public ServiceUtil(uint timeout, List<ServiceInfo> info)
        {
            this.timeout = timeout;
            this.ServicesInfo = info;
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

        private List<ServiceInfo> serviceInfo;

        public List<ServiceInfo> ServicesInfo
        {
            get
            {
                return serviceInfo;
            }
            set
            {
                serviceController = new ServiceController();
                serviceInfo = value;
            }
        }

        private bool processing = false;

        private ServiceController serviceController;

        public void ProcessServices()
        {
            if (this.ServicesInfo != null)
            {
                processing = true;

                processIdx = 0;

                processServices();
            }
        }

        private int processIdx;

        private void processServices()
        {
            if (processIdx < ServicesInfo.Count())
            {
                this.Progress += new ServiceEventHandler(progressHandler);

                ServiceInfo serviceInfo = this.ServicesInfo[processIdx];

                serviceController.ServiceName = serviceInfo.Name;

                bool enable = serviceInfo.Status.Equals(ServiceControllerStatus.Running);

                enableService(enable);
                setServiceStatus(serviceInfo.Status);
            }
        }

        private const int TOTAL_TRY = 5;

        private void setServiceStatus(ServiceControllerStatus status)
        {
            if (serviceController.Status.Equals(status)) //already in the wanted status
            {
                OnProgress(processIdx + 1);
            }
            else
            {
                try
                {
                    TimeSpan timeoutSec = TimeSpan.FromSeconds(timeout);

                    bool tryAgain = false;
                    int tryCount = 0;

                    do
                    {
                        try
                        {
                            if (status.Equals(ServiceControllerStatus.Running))
                                serviceController.Start(); //TODO: support arguments
                            else
                                serviceController.Stop();
                        }
                        catch (InvalidOperationException e)
                        {
                            //in some cases, the service status might be changed although the exception is thrown
                            Thread.Sleep(TimeSpan.FromSeconds(5)); //wait a little bit time to try again
                            
                            serviceController.Refresh();
                            if (serviceController.Status.Equals(status)) //equal
                                tryAgain = false;
                            else if (tryCount < TOTAL_TRY) //not equal, not reach total
                                tryAgain = true;
                            else //when not equal to status, AND reach total try
                                throw e;

                            tryCount++;
                        }
                    } while (tryAgain);

                    serviceController.WaitForStatus(status, timeoutSec);

                    Console.WriteLine(serviceController.Status.ToString());

                    //check the status to make sure it is started properly
                    if (serviceController.Status.Equals(status))
                    {
                        OnProgress(processIdx + 1);
                    }
                    else
                    {
                        throw new InvalidOperationException(serviceController.ServiceName + " cannot be changed to status " + status.ToString());
                    }
                }
                //TODO: there should be configuration for the app
                //when error occurs to a service, should it stop or continue to next service?
                catch (InvalidOperationException e)
                {
                    OnError(e);
                }
                catch (System.ServiceProcess.TimeoutException e)
                {
                    OnError(e);
                }
            }
        }

        private void enableService(bool enabled)
        {
            try
            {
                if(enabled)
                    ChangeStartMode(serviceController, ServiceStartMode.Manual);
                else
                    ChangeStartMode(serviceController, ServiceStartMode.Disabled);
            }
            catch (ExternalException e)
            {
                OnError(e);
            }
        }

        private void progressHandler(object sender, EventArgs e)
        {
            this.Progress -= progressHandler;

            //TODO: deal with error, when error occurs, maybe skip the service and go to next

            processIdx++;
            if (processIdx < this.ServicesInfo.Count())
                processServices();
            else
                OnComplete();
        }

        protected void OnError(Exception e)
        {
            if (e != null)
            {
                Console.WriteLine(e.ToString());
            }

            if (Error != null)
                Error(this, EventArgs.Empty);
        }

        protected void OnProgress(int idx)
        {
            if (Progress != null)
            {
                ServiceProcessProgressEventArgs arg = new ServiceProcessProgressEventArgs();
                arg.NumCompleted = idx;
                arg.Total = this.ServicesInfo.Count();
                Progress(this, arg);
            }
        }

        protected void OnComplete()
        {
            processing = false;

            serviceController.Close();
            serviceController.Dispose();

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        
        #region the methods to set service mode
        /// <summary>
        /// See http://www.codeproject.com/Articles/7665/Extend-ServiceController-class-to-change-the-Start
        /// </summary>
        /// <param name="serviceController"></param>
        /// <param name="mode"></param>
        private void ChangeStartMode(ServiceController serviceController, ServiceStartMode mode)
        {
            /*if (value != "Automatic" && value != "Manual" && value != "Disabled")
                throw new Exception("The valid values are Automatic, Manual or Disabled");*/

            //construct the management path
            Console.WriteLine("change service " + serviceController.ServiceName + " to " + mode.ToString());
            string path = "Win32_Service.Name='" + serviceController.ServiceName + "'";
            ManagementPath p = new ManagementPath(path);
            //construct the management object
            ManagementObject ManagementObj = new ManagementObject(p);
            //we will use the invokeMethod method of the ManagementObject class
            object[] parameters = new object[1];
            parameters[0] = mode.ToString();
            object result = ManagementObj.InvokeMethod("ChangeStartMode", parameters);

            Console.WriteLine("result: " + result);
        }

        #endregion
    } //class end

    public class ServiceProcessProgressEventArgs:EventArgs
    {
        public int NumCompleted { get; set; }
        public int Total { get; set; }
    }
}
