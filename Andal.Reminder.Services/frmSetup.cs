using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Andal.CommonLibrary;
using Andal.ESS.BaseDataAccess;

namespace Andal.Reminder.Services
{
    public partial class frmSetup : Form
    {

        string sServiceName = "Andal Reminder Service";
        public frmSetup()
        {
            InitializeComponent();
        }

        private void txtServerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtDatabaseName.SelectionStart = 0;
                txtDatabaseName.SelectionLength = txtDatabaseName.TextLength;
                txtDatabaseName.Focus();
            }
        }

        private void txtDatabaseName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtUserName.SelectionStart = 0;
                txtUserName.SelectionLength = txtUserName.TextLength;
                txtUserName.Focus();
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPassword.SelectionStart = 0;
                txtPassword.SelectionLength = txtPassword.TextLength;
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOk.Focus();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string sServerName = this.txtServerName.Text;
            string sDatabaseName = this.txtDatabaseName.Text;
            string sUserName = this.txtUserName.Text;
            string sPassword = this.txtPassword.Text;

            if (sServerName == string.Empty && sDatabaseName == string.Empty && sUserName == string.Empty)
            {
                if (sServerName == string.Empty)
                {
                    MessageBox.Show("Please specify Server Name", sServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtServerName.Focus();
                    return;
                }
                if (sDatabaseName == string.Empty)
                {
                    MessageBox.Show("Please specify Database Name", sServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtDatabaseName.Focus();
                    return;
                }
                if (sUserName == string.Empty)
                {
                    MessageBox.Show("Please specify User Name", sServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUserName.Focus();
                    return;
                }
            }

            string strConn = string.Format(
                BaseDataShareESS.ConnectionString,
                sServerName,
                sDatabaseName,
                sUserName,
                sPassword);

            if (CheckConnection(strConn))
            {
                MessageBox.Show("Connection is successfully", sServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                StreamWriter oStreamWriter = File.CreateText(BaseDataShareESS.FileConfig);
                oStreamWriter.WriteLine(Cryptography.EncryptString(sServerName));
                oStreamWriter.WriteLine(Cryptography.EncryptString(sDatabaseName));
                oStreamWriter.WriteLine(Cryptography.EncryptString(sUserName));
                oStreamWriter.WriteLine(Cryptography.EncryptString(sPassword));

                oStreamWriter.Flush();
                oStreamWriter.Close();
                oStreamWriter = null;
                //this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("SQL Server doesn't exists!", sServiceName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //this.DialogResult = DialogResult.None;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            System.Windows.Forms.Application.Exit();
        }

        private bool CheckConnection(string strConn)
        {
            bool bStatus = false;
            Andal.CommonLibrary.IBaseDACL oLocalDataAccessDACLESS = null;
            try
            {
                oLocalDataAccessDACLESS = new LocalDataAccessDACLESS(strConn);
                bStatus = oLocalDataAccessDACLESS.CheckConnection();

            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }
            finally
            {
                if (oLocalDataAccessDACLESS != null) oLocalDataAccessDACLESS.Dispose();
                oLocalDataAccessDACLESS = null;
            }

            return bStatus;
        }
    }
}
