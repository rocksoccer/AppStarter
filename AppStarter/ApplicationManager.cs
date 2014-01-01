using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AppStarter
{
    public delegate void ApplicationManagerEventHandler(object sender, EventArgs e);

    class ApplicationManager
    {
        public ApplicationManager()
        {
            apps = new List<ApplicationInfo>();
        }

        public event ApplicationManagerEventHandler AllAppsExited;

        private readonly List<ApplicationInfo> apps;

        public void StartApplication(string configFile)
        {
            //start in separate thread
            ApplicationInfo appInfo = new ApplicationInfo(XDocument.Load(configFile));
            
            appInfo.Completed += appInfo_Completed;
            //when the app is about to stop services, it should check whether the service is used by other programs
            appInfo.ServiceCheckCallback = new ServiceCheckFunction(IsServiceUsedByOtherApps);
            apps.Add(appInfo);

            appInfo.Start();
        }

        private void appInfo_Completed(object sender, EventArgs e)
        {
            //remove the app from the list
            ApplicationInfo appInfo = (ApplicationInfo) sender;
            apps.Remove(appInfo);

            //dispatch event when no more application left in the list
            if (apps.Count == 0 && AllAppsExited != null)
            {
                AllAppsExited(this, EventArgs.Empty);
            }
        }

        private bool IsServiceUsedByOtherApps(ApplicationInfo sender, string serviceName)
        {
            return apps.FindIndex(app => (app != sender && app.IsServiceUsed(serviceName))) >= 0;
        }
    } //class end
}
