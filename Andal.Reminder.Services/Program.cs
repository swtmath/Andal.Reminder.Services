using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//using System.Configuration;

namespace Andal.Reminder.Services
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //string xxx = ConfigurationSettings.AppSettings["MailToEmployee"].ToString();

            Application.Run(new frmMain());
        }
    }
}
