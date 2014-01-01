using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AppStarter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            appMgr = new ApplicationManager();
            appMgr.AllAppsExited += appMgr_AllAppsExited;

            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            hideForm();
        }

        private ApplicationManager appMgr;
        
        public void StartApplication(string configFile)
        {
            appMgr.StartApplication(configFile);
        }

        void appMgr_AllAppsExited(object sender, EventArgs e)
        {
            exit();
        }


        #region The methods for form
        private void hideForm()
        {
            this.Hide();
            this.ShowInTaskbar = false;
        }

        private void showForm()
        {
            WindowState = FormWindowState.Normal;
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            showForm();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!isRealClose)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    hideForm();
                    e.Cancel = true;
                }
            }
            else
            {
                isRealClose = false;
            }
        }

        private bool isRealClose = false;
        private void exit()
        {
            isRealClose = true;
            this.Close();
        }

        private void notifyIconExitMenuItem_Click(object sender, EventArgs e)
        {
            exit();
        }
        #endregion
    } //class end
}
