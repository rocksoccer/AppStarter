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
        public static void Main(string[] args)
        {
            if (args.Count()==1)
            {
                ApplicationInfo appInfo = new ApplicationInfo(XDocument.Load(args[0])); //"test.xml"
                appInfo.Start();
            }
            //TODO: when no param is given, show the UI
        }
    }
}
