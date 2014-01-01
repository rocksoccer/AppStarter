using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AppStarter
{
    class MainApplication
    {
        static Mutex mutex = new Mutex(true, "d091402b-6b1a-40a8-8a6e-161d9140a491");

        private static MainForm mainForm;

        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                mainForm=new MainForm();

                if (args.Count() == 1)
                {
                    mainForm.StartApplication(args[0]);
                }

                Application.Run(mainForm);

                mutex.ReleaseMutex();
            }
            else
            {
                Console.WriteLine("we already have an instance");
            }
        }
    }
}
