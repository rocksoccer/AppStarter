using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppStarter
{
    class MainApplication
    {
        public static void Main()
        {
            ApplicationInfo appInfo = new ApplicationInfo(XDocument.Load("test.xml"));
            appInfo.Start();
        }
    }
}
