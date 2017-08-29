using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Andal.CommonLibrary;

namespace Andal.Reminder.Services
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private string LoadConfig()
        {
            string temp = Andal.ESS.BaseDataAccess.BaseDataShareESS.GetFileConfig(typeof(Andal.Reminder.Services.frmMain).Assembly.Location.Substring(0, typeof(Andal.Reminder.Services.frmMain).Assembly.Location.LastIndexOf(@"\") + 1));
            return temp;
        }

        private void Run()
        {
            string temp = LoadConfig();
            if (temp == string.Empty)
            {
                using (frmSetup ofrmSetup = new frmSetup())
                {
                    ofrmSetup.ShowDialog();
                }
                temp = LoadConfig();
            }

            ServicesDataAccess oIBaseDACL = null;
            try
            {
                oIBaseDACL = new ServicesDataAccess(temp);
                oIBaseDACL.BeginTrans();
                oIBaseDACL.RunProcess(null);
                oIBaseDACL.CommitTrans();

            }
            catch (Exception oException)
            {
                
                if (oIBaseDACL != null)
                    oIBaseDACL.AbortTrans();
            }
            finally
            {
                if (oIBaseDACL != null) oIBaseDACL.Dispose();
                oIBaseDACL = null;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Run();
            this.Close();
            System.Windows.Forms.Application.Exit();
        }

    }
}
