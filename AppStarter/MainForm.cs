using System;
using System.Windows.Forms;

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

        public void CreateNotifyIcon()
        {
            if (notifyIcon == null)
            {
                this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
                this.notifyIconMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
                this.notifyIconExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.notifyIconMenuStrip.SuspendLayout();

                // 
                // notifyIcon
                // 
                this.notifyIcon.ContextMenuStrip = this.notifyIconMenuStrip;
                this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
                this.notifyIcon.Text = "App Starter";
                this.notifyIcon.Visible = true;
                this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
                // 
                // notifyIconMenuStrip
                // 
                this.notifyIconMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.notifyIconExitMenuItem
                });
                this.notifyIconMenuStrip.Name = "notifyIconMenuStrip";
                this.notifyIconMenuStrip.Size = new System.Drawing.Size(93, 26);
                // 
                // notifyIconExitMenuItem
                // 
                this.notifyIconExitMenuItem.Name = "notifyIconExitMenuItem";
                this.notifyIconExitMenuItem.Size = new System.Drawing.Size(92, 22);
                this.notifyIconExitMenuItem.Text = "Exit";
                this.notifyIconExitMenuItem.Click += new System.EventHandler(this.notifyIconExitMenuItem_Click);

                this.notifyIconMenuStrip.ResumeLayout(false);
            }
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
