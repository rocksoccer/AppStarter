using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace AppStarter
{
    class MainApplication
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SingleInstanceApplication.Run(new MainForm(), StartupHandler, StartupNextInstanceHandler);
        }

        static void StartupHandler(object sender, StartupEventArgs e)
        {
            SingleInstanceApplication app = (SingleInstanceApplication) sender;
            MainForm form = (MainForm) app.AppMainForm;

            form.CreateNotifyIcon();

            processCommandLine(form, e.CommandLine);
        }

        static void StartupNextInstanceHandler(object sender, StartupNextInstanceEventArgs e)
        {
            SingleInstanceApplication app = (SingleInstanceApplication)sender;
            MainForm form = (MainForm)app.AppMainForm;

            processCommandLine(form, e.CommandLine);
        }

        static void processCommandLine(MainForm form, ReadOnlyCollection<string> cmdLine)
        {
            if (cmdLine.Count == 2)
            {
                form.StartApplication(cmdLine[1]);
            }
        }
    }

    class SingleInstanceApplication : WindowsFormsApplicationBase
    {
        private SingleInstanceApplication()
        {
            base.IsSingleInstance = true;
        }

        public static void Run(Form f, StartupEventHandler startupHandler, StartupNextInstanceEventHandler StartupNextInstanceHandler)
        {
            SingleInstanceApplication app = new SingleInstanceApplication(); 
            app.MainForm = f;
            app.Startup += startupHandler;
            app.StartupNextInstance += StartupNextInstanceHandler; 
            app.Run(Environment.GetCommandLineArgs());
        }

        public Form AppMainForm
        {
            get
            {
                return MainForm;
            }
        }
    }
}
