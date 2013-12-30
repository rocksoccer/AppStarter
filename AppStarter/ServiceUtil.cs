using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Runtime.InteropServices;

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

                enableService(serviceInfo.Status.Equals(ServiceControllerStatus.Running));
                setServiceStatus(serviceInfo.Status);
            }
        }

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

                    if(status.Equals(ServiceControllerStatus.Running))
                        serviceController.Start(); //TODO: support arguments
                    else
                        serviceController.Stop();
                    serviceController.WaitForStatus(status, timeoutSec);

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
                catch (InvalidOperationException)
                {
                    OnError();
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    OnError();
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
            catch (ExternalException)
            {
                OnError();
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
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            String lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(
            IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        private static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        /// <summary>
        /// See http://peterkellyonline.blogspot.de/2011/04/configuring-windows-service.html
        /// </summary>
        /// <param name="mode"></param>
        private void ChangeStartMode(ServiceController serviceController, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }

            var serviceHandle = OpenService(
                scManagerHandle,
                serviceController.ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }

            var result = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint)mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                throw new ExternalException("Could not change service start type: "
                    + win32Exception.Message);
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }

        #endregion
    } //class end

    public class ServiceProcessProgressEventArgs:EventArgs
    {
        public int NumCompleted { get; set; }
        public int Total { get; set; }
    }
}
