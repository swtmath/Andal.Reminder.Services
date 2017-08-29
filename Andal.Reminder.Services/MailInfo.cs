using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andal.Reminder.Services
{
    public class MailInfo
    {
        private string _Admin = string.Empty;
        private string _AdminEmail = string.Empty;

        public string Admin
        {

            get { return _Admin; }
            set { _Admin = value; }
        }
        public string AdminEmail
        {

            get { return _AdminEmail; }
            set { _AdminEmail = value; }
        }
    }
}
