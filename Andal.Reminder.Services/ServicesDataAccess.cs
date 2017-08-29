using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Andal.CommonLibrary;
using Andal.ESS.BaseDataAccess;
using System.Threading;
using DotNetNuke;
//using DotNetNuke.Services.Mail;
//using DotNetNuke.Common;
//using DotNetNuke.Common.Utilities;
//using DotNetNuke.Entities.Portals;
//using DotNetNuke.Entities.Tabs;
//using DotNetNuke.Entities.Users;
//using DotNetNuke.Framework.Providers;
//using DotNetNuke.Services.FileSystem;
using System.Configuration;
using System.Net.Mail;
using DotNetNuke.Common.Utilities;
using System.Net;

namespace Andal.Reminder.Services
{
    class ServicesDataAccess : Andal.ESS.BaseDataAccess.LocalDataAccessDACLESS, Andal.CommonLibrary.IBaseDACL
    {
        //private MailInfo _MailInfo;
        private string _SMTPServer = string.Empty;
        private string _SMTPAuthentication = string.Empty;
        private string _SMTPUsername = string.Empty;
        private string _SMTPPassword = string.Empty;

        public ServicesDataAccess(string sConn)
            : base(sConn)
        { 
            //_SourceTypeESS = SourceTypeESS.ApprovalLeave;
        }

        public JobStructureList LoadsJobStructure(Guid EmployeeId)
        {
            JobStructureList oJobStructureList = new JobStructureList();

            //load by employee -> karena employee dalam job ini bisa banyak
            string CmdText = "  SELECT _job.[Id],_job.[Name],Email, "
                           + " _job.[Description],_job.[ParentId],_job.[Level], "
                           + " _Employee.[FullName] as EmployeeFullname,_employee.[Id] as EmployeeId,[_Employee].[Number] as EmployeeNumber "
                           + " FROM _employee INNER JOIN [_Job] ON _job.Id= _employee.[JobId] where _employee.Id = @Id";
            ArrayList oArrayList = new ArrayList();
            oArrayList.Add(this.SetParameterValue("@Id", DbType.Guid, EmployeeId));
            DataTable oDataTable = ExecuteDataTable(CmdText, (Parameter[])oArrayList.ToArray(typeof(Parameter)));

            JobStructure oJobStructure = new JobStructure();
            foreach (DataRow oDataRow in oDataTable.Rows)
            {
                if (oDataRow["Id"] != DBNull.Value) oJobStructure.JobId = ConvertObjectToGuid(oDataRow["Id"]);
                if (oDataRow["Name"] != DBNull.Value) oJobStructure.JobName = ConvertObjectToString(oDataRow["Name"]);
                if (oDataRow["Description"] != DBNull.Value) oJobStructure.JobDescription = ConvertObjectToString(oDataRow["Description"]);
                if (oDataRow["ParentId"] != DBNull.Value) oJobStructure.JobParentId = ConvertObjectToGuid(oDataRow["ParentId"]);
                if (oDataRow["Level"] != DBNull.Value) oJobStructure.JobLevel = ConvertObjectToInt(oDataRow["Level"]);
                if (oDataRow["EmployeeId"] != DBNull.Value) oJobStructure.EmployeeId = ConvertObjectToGuid(oDataRow["EmployeeId"]);
                if (oDataRow["EmployeeFullname"] != DBNull.Value) oJobStructure.EmployeeName = ConvertObjectToString(oDataRow["EmployeeFullname"]);
                if (oDataRow["EmployeeNumber"] != DBNull.Value) oJobStructure.EmployeeNumber = ConvertObjectToString(oDataRow["EmployeeNumber"]);
                if (oDataRow["Email"] != DBNull.Value) oJobStructure.EmployeeMail = ConvertObjectToString(oDataRow["Email"]);
                oJobStructureList.Add(oJobStructure);
            }

            //load by Job
            if (oJobStructure.JobParentId != Guid.Empty) GenerateJob(oJobStructure.JobParentId, ref oJobStructureList);
            return oJobStructureList;
        }

        private void GenerateJob(Guid JobId, ref JobStructureList oJobStructureList)
        {
            string CmdText = " SELECT _job.[Id],_job.[Name], "
                            + " _job.[Description],_job.[ParentId],_job.[Level], "
                            + " (SELECT TOP 1 [_Employee].[Id] FROM _employee WHERE _job.[Id] = _employee.[JobId] AND status !=4 ORDER BY [_Employee].[EndEffectiveDate],[_Employee].[EffectiveDate]) AS EmployeeId, "
                            + " (SELECT TOP 1 [_Employee].[Fullname] FROM _employee WHERE _job.[Id] = _employee.[JobId] AND status !=4 ORDER BY [_Employee].[EndEffectiveDate],[_Employee].[EffectiveDate]) AS EmployeeFullname, "
                            + " (SELECT TOP 1 [_Employee].[Email] FROM _employee WHERE _job.[Id] = _employee.[JobId] AND status !=4 ORDER BY [_Employee].[EndEffectiveDate],[_Employee].[EffectiveDate]) AS Email, "
                            + " (SELECT TOP 1 [_Employee].[Number] FROM _employee WHERE _job.[Id] = _employee.[JobId] AND status !=4 ORDER BY [_Employee].[EndEffectiveDate],[_Employee].[EffectiveDate]) AS EmployeeNumber "
                            + " FROM _Job  where _job.Id = @Id ";
            ArrayList oArrayList = new ArrayList();
            oArrayList.Add(this.SetParameterValue("@Id", DbType.Guid, JobId));
            DataTable oDataTable = ExecuteDataTable(CmdText, (Parameter[])oArrayList.ToArray(typeof(Parameter)));
            JobStructure oJobStructure = new JobStructure();
            foreach (DataRow oDataRow in oDataTable.Rows)
            {
                if (oDataRow["Id"] != DBNull.Value) oJobStructure.JobId = ConvertObjectToGuid(oDataRow["Id"]);
                if (oDataRow["Name"] != DBNull.Value) oJobStructure.JobName = ConvertObjectToString(oDataRow["Name"]);
                if (oDataRow["Description"] != DBNull.Value) oJobStructure.JobDescription = ConvertObjectToString(oDataRow["Description"]);
                if (oDataRow["ParentId"] != DBNull.Value) oJobStructure.JobParentId = ConvertObjectToGuid(oDataRow["ParentId"]);
                if (oDataRow["Level"] != DBNull.Value) oJobStructure.JobLevel = ConvertObjectToInt(oDataRow["Level"]);
                if (oDataRow["EmployeeId"] != DBNull.Value) oJobStructure.EmployeeId = ConvertObjectToGuid(oDataRow["EmployeeId"]);
                if (oDataRow["EmployeeFullname"] != DBNull.Value) oJobStructure.EmployeeName = ConvertObjectToString(oDataRow["EmployeeFullname"]);
                if (oDataRow["EmployeeNumber"] != DBNull.Value) oJobStructure.EmployeeNumber = ConvertObjectToString(oDataRow["EmployeeNumber"]);
                if (oDataRow["Email"] != DBNull.Value) oJobStructure.EmployeeMail = ConvertObjectToString(oDataRow["Email"]);
                oJobStructureList.Add(oJobStructure);
            }
            if (oJobStructure.JobParentId != Guid.Empty) GenerateJob(oJobStructure.JobParentId, ref oJobStructureList);
        }

        //private void GetMailInfo()
        //{
        //    IDataReader oIDataReader = this.ExecuteDataReader("SELECT * FROM dn_users (Nolock) Where username='admin'");
        //    if (oIDataReader.Read())
        //    {
        //        _MailInfo = new MailInfo();
        //        _MailInfo.Admin = oIDataReader["DisplayName"].ToString();
        //        _MailInfo.AdminEmail = oIDataReader["Email"].ToString();
        //    }
        //    this.CloseDataReader(ref oIDataReader);
        //}

        private void LoadSMTPConfiguration()
        {
            DataTable oDataTable = this.ExecuteDataTable("SELECT * FROM ES_HostSettings WHERE SettingName LIKE 'SMTP%'");
            foreach (DataRow oDataRow in oDataTable.Rows)
            {
                if (oDataRow["SettingName"].ToString() == "SMTPServer")
                {
                    _SMTPServer = oDataRow["SettingValue"].ToString();
                }
                else if (oDataRow["SettingName"].ToString() == "SMTPAuthentication")
                {
                    _SMTPAuthentication = oDataRow["SettingValue"].ToString();
                }
                else if (oDataRow["SettingName"].ToString() == "SMTPUsername")
                {
                    _SMTPUsername = oDataRow["SettingValue"].ToString();
                }
                else if (oDataRow["SettingName"].ToString() == "SMTPPassword")
                {
                    _SMTPPassword = oDataRow["SettingValue"].ToString();
                }
            }
            this.CloseDataTable(ref oDataTable);
        }

        private void LocalLoadSystemInfo()
        {
            string CmdText = "SELECT HistoryRoster, AttendanceLastRunDate, PayrollLastRunDate, PayrollRequirePolling, WeekWorkingDay, DayWorkingHour, "
                    + " MaxOverTimeWeeks, MaxOverTimeDays, MaxOverTimeMonth, "
                    + " LeaveDayProportional, "
                    + " ClaimAllowMinus, ClaimDayProportional "
                    + " FROM _SystemInfo (NoLock) ";
            CmdText = string.Format(CmdText);
            DataSet oDataSet = this.ExecuteDataSet(CmdText);
            Andal.ESS.BaseDataAccess.SystemInfo oRaw = new Andal.ESS.BaseDataAccess.SystemInfo();
            Andal.ESS.BaseDataAccess.Branch oRaw1 = new Andal.ESS.BaseDataAccess.Branch();
            if (oDataSet.Tables[0].Rows.Count > 0)
            {
                DataRow oDataRow = oDataSet.Tables[0].Rows[0];

                if (oDataRow["HistoryRoster"] != DBNull.Value) oRaw.HistoryRoster = ConvertObjectToBoolean(oDataRow["HistoryRoster"]);
                if (oDataRow["MaxOverTimeMonth"] != DBNull.Value) oRaw.MaxOverTimeMonth = ConvertObjectToInt(oDataRow["MaxOverTimeMonth"]);
                if (oDataRow["PayrollRequirePolling"] != DBNull.Value) oRaw.PayrollRequirePolling = ConvertObjectToBoolean(oDataRow["PayrollRequirePolling"]);
                oRaw.LeaveDayProportional = ConvertObjectToInt(oDataRow["LeaveDayProportional"]);
                if (oDataRow["MaxOverTimeDays"] != DBNull.Value) oRaw.MaxOverTimeDays = ConvertObjectToInt(oDataRow["MaxOverTimeDays"]);
                if (oDataRow["MaxOverTimeWeeks"] != DBNull.Value) oRaw.MaxOverTimeWeeks = ConvertObjectToInt(oDataRow["MaxOverTimeWeeks"]);
                if (oDataRow["ClaimAllowMinus"] != DBNull.Value) oRaw.ClaimAllowMinus = ConvertObjectToBoolean(oDataRow["ClaimAllowMinus"]);
                if (oDataRow["ClaimDayProportional"] != DBNull.Value) oRaw.ClaimDayProportional = ConvertObjectToInt(oDataRow["ClaimDayProportional"]);
                if (oDataRow["DayWorkingHour"] != DBNull.Value) oRaw.DayWorkingHour = ConvertObjectToInt(oDataRow["DayWorkingHour"]);
                if (oDataRow["WeekWorkingDay"] != DBNull.Value) oRaw.WeekWorkingDay = ConvertObjectToInt(oDataRow["WeekWorkingDay"]);
                if (oDataRow["PayrollLastRunDate"] != DBNull.Value) oRaw.PayrollLastRunDate = ConvertObjectToDateTime(oDataRow["PayrollLastRunDate"]);
                if (oDataRow["AttendanceLastRunDate"] != DBNull.Value) oRaw.AttendanceLastRunDate = ConvertObjectToDateTime(oDataRow["AttendanceLastRunDate"]);

            }
            _SystemInfo = oRaw;
            this.CloseDataSet(ref oDataSet);
        }
        public string ConvertHTMLToText(string sHTML)
        {
            string sContent = sHTML;
            sContent = sContent.Replace("<br />", Environment.NewLine);
            sContent = sContent.Replace("<br>", Environment.NewLine);
            sContent = HtmlUtils.FormatText(sContent, true);
            return HtmlUtils.StripTags(sContent, true);
        }

        private string SendMailInternal(MailMessage mailMessage, string subject, string body, System.Net.Mail.MailPriority priority,
                                DotNetNuke.Services.Mail.MailFormat bodyFormat, Encoding bodyEncoding, IEnumerable<Attachment> attachments,
                                string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            string retValue;

            mailMessage.Priority = (System.Net.Mail.MailPriority)priority;
            mailMessage.IsBodyHtml = (bodyFormat == DotNetNuke.Services.Mail.MailFormat.Html);

            //attachments
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    mailMessage.Attachments.Add(attachment);
                }
            }

            //message
            mailMessage.SubjectEncoding = bodyEncoding;
            mailMessage.Subject = HtmlUtils.StripWhiteSpace(subject, true);
            mailMessage.BodyEncoding = bodyEncoding;

            //added support for multipart html messages
            //add text part as alternate view
            var PlainView = AlternateView.CreateAlternateViewFromString(ConvertHTMLToText(body), null, "text/plain");
            mailMessage.AlternateViews.Add(PlainView);
            if (mailMessage.IsBodyHtml)
            {
                var HTMLView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                mailMessage.AlternateViews.Add(HTMLView);
            }

            if (!String.IsNullOrEmpty(smtpServer))
            {
                try
                {
                    var smtpClient = new SmtpClient();

                    var smtpHostParts = smtpServer.Split(':');
                    smtpClient.Host = smtpHostParts[0];
                    if (smtpHostParts.Length > 1)
                    {
                        smtpClient.Port = Convert.ToInt32(smtpHostParts[1]);
                    }

                    switch (smtpAuthentication)
                    {
                        case "":
                        case "0": //anonymous
                            break;
                        case "1": //basic
                            if (!String.IsNullOrEmpty(smtpUsername) && !String.IsNullOrEmpty(smtpPassword))
                            {
                                smtpClient.UseDefaultCredentials = false;
                                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                            }
                            break;
                        case "2": //NTLM
                            smtpClient.UseDefaultCredentials = true;
                            break;
                    }
                    smtpClient.EnableSsl = smtpEnableSSL;
                    smtpClient.Send(mailMessage);
                    retValue = "";
                }
                catch (SmtpFailedRecipientException exc)
                {
                    retValue = "SendMailInternal failed : FailedRecipient : " + exc.Message;
                }
                catch (SmtpException exc)
                {
                    retValue = "SendMailInternal failed : SMTPConfigurationProblem : " + exc.Message;
                }
                catch (Exception exc)
                {
                    retValue = exc.Message;
                }
                finally
                {
                    mailMessage.Dispose();
                }
            }
            else
            {
                retValue = "SendMailInternal failed : SMTPConfigurationProblem";
            }

            return retValue;
        }

        public string SendMail(string mailFrom, string mailTo, string cc, string bcc, string replyTo, System.Net.Mail.MailPriority priority, string subject, DotNetNuke.Services.Mail.MailFormat bodyFormat, Encoding bodyEncoding,
                                      string body, List<Attachment> attachments, string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
            //translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
            mailTo = mailTo.Replace(";", ",");
            cc = cc.Replace(";", ",");
            bcc = bcc.Replace(";", ",");

            MailMessage mailMessage = null;
            mailMessage = new MailMessage { From = new MailAddress(mailFrom) };
            if (!String.IsNullOrEmpty(mailTo))
            {
                mailMessage.To.Add(mailTo);
            }
            if (!String.IsNullOrEmpty(cc))
            {
                mailMessage.CC.Add(cc);
            }
            if (!String.IsNullOrEmpty(bcc))
            {
                mailMessage.Bcc.Add(bcc);
            }
            if (replyTo != string.Empty)
            {
#pragma warning disable 612,618
                mailMessage.ReplyTo = new MailAddress(replyTo);
#pragma warning restore 612,618
            }

            return SendMailInternal(mailMessage, subject, body, priority, bodyFormat, bodyEncoding,
                attachments, smtpServer, smtpAuthentication, smtpUsername, smtpPassword, smtpEnableSSL);
        }

        public bool RunProcess(object Object)
        {
            LoadSMTPConfiguration();
            LocalLoadSystemInfo();

            DateTime oToday = GetDateServer();
            ArrayList oArrayList = new ArrayList();

            //GetMailInfo();

            string CmdText = string.Empty;
            string CmdText1 = string.Empty;
            string sContent = string.Empty;
            string sMailTemplateId = string.Empty;

            try
            {
                #region alert pension
                CmdText = " SELECT DATEDIFF(YEAR,DateOfBirth,GETDATE()), DateOfBirth, EmployeeFullName, "
                        + " JobDescription, DepartmentDescription, JoinDate, datediff(YEAR,JoinDate,GETDATE()) AS [MasaKerja] "
                        + "  FROM vEmployee (nolock) "
                        + " WHERE Status!=4 And ResignDate IS NULL "
                        + " AND DateDiff(YEAR,DateOfBirth,GETDATE()) =  "
                        + " (SELECT PensionMAge FROM _SystemInfo (nolock)) "
                        + " ORDER BY DateDiff(YEAR,JoinDate,GETDATE()) DESC  ";
                DataTable oDataTablePension = this.ExecuteDataTable(CmdText);
                if (oDataTablePension.Rows.Count > 0 && GetDateServer().Day == 1 && GetDateServer().Month == 1)
                {
                    CmdText1 = " SELECT Email, FullName FROM [ESS_SystemInfoDashBoardNotificationAdmin] (nolock) "
                        + " INNER JOIN [_Employee] ON ESS_SystemInfoDashBoardNotificationAdmin.Id = _Employee.Id "
                        + " WHERE ESS_SystemInfoDashBoardNotificationAdmin.SourceType = {0} ";
                    CmdText1 = string.Format(CmdText1, (int)SourceTypeESS.AlertPension);
                    DataTable oDataTableAdmin = ExecuteDataTable(CmdText1);

                    //sContent = "Berikut kami sampaikan nama karyawan yang telah memiliki masa bakti pada " + GetDateServer().ToString("MMM yyyy") + " : ";
                    sContent = "<BR/><BR/><Table border=\"1\" cellspacing=\"0\" cellpadding=\"3\">"
                        + " <tr><td><FONT face=Tahoma size=2>NO.</Font></td><td><FONT face=Tahoma size=2>NAMA</Font></td> "
                        + " <td><FONT face=Tahoma size=2>JABATAN</Font></td>"
                        + " <td><FONT face=Tahoma size=2>DIVISI</Font></td>"
                        + " <td><FONT face=Tahoma size=2>JOIN DATE</Font></td>"
                        + " <td><FONT face=Tahoma size=2>MASA KERJA</Font></td>"
                        + " <td><FONT face=Tahoma size=2>TGL PENSIUN</Font></td>"
                        + " </tr>";

                    int i = 1;
                    foreach (DataRow oDataRow in oDataTablePension.Rows)
                    {
                        DateTime PensionDate = new DateTime(GetDateServer().Year, ConvertObjectToDateTime(oDataRow["JoinDate"]).Month, ConvertObjectToDateTime(oDataRow["JoinDate"]).Day);
                        sContent += " <tr><td><FONT face=Tahoma size=2>" + i.ToString() + "</Font></td><td><FONT face=Tahoma size=2>" + oDataRow["EmployeeFullName"].ToString() + "</Font></td> "
                        + " <td><FONT face=Tahoma size=2>" + oDataRow["JobDescription"].ToString() + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + oDataRow["DepartmentDescription"].ToString() + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + ConvertObjectToDateTime(oDataRow["JoinDate"]).ToString("dd MMM yyyy") + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + oDataRow["MasaKerja"].ToString() + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + PensionDate.ToString("dd MMM yyyy") + "</Font></td>"
                        + " </tr>";
                        i++;
                    }
                    sContent += "</Table>";

                    sMailTemplateId = ConfigurationSettings.AppSettings["EmailToAdminForPensionListAlert"].ToString();

                    if (sMailTemplateId != string.Empty)
                    {
                        foreach (DataRow drAdmin in oDataTableAdmin.Rows)
                        {
                            DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                            if (oDataTableMailTemplate.Rows.Count > 0)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforPensionNContractProbation(sBody, drAdmin["FullName"].ToString(), GetDateServer().Year.ToString(), sContent);
                                    LocalSendMail(_SMTPUsername, drAdmin["Email"].ToString(), sSubject, sBody, null, _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);

                                }
                            }
                        }
                    }

                }

                #endregion

                #region alert probation/contract
                CmdText = " SELECT "
                    + " A.*, _Employee.FullName AS EmployeeFullName, _Employee.Email as EmployeeEmail "
                    + " FROM EmployeeTransaction A (nolock) "
                    + " INNER JOIN _Employee (nolock) ON A.EmployeeId = _Employee.Id "
                    + " WHERE A.ReferenceId = '" + ConfigurationSettings.AppSettings["AlertContractProbationRefId"].ToString() + "'";
                DataTable oDataTable = this.ExecuteDataTable(CmdText);

                CmdText = " SELECT A.Id AS EmployeeId, A.Gender,A.Number As \"Employee ID\", A.FullName AS \"EmployeeFullname\",  "
                    + " A.JoinDate AS JoinDate, Datediff(DAY,GETDATE(),A.EndEffectiveDate) AS DayRange, "
                    + " dbo.fn_EmployeeStatus(A.EmployeeStatus) AS EmployeeStatus, A.EmployeeStatus AS EmployeeStatusId, A.Id, "
                    + " A.EndEffectiveDate AS \"End Contract/Probation/Expire Date\", AA.ID AS TransactionId, AA.Number AS RegNo, "
                    + " 'Status Alteration' AS Type, " + Convert.ToInt32(SourceType.StatusAlteration) + " AS TypeId "
                    + "  FROM _Employee A (NoLock) "
                    + " LEFT JOIN ( "
                    + " SELECT Number, EmployeeId, EffectiveDate, FromType, StatusAlteration.ID  FROM StatusAlteration (NoLock) "
                    + " INNER JOIN EmployeeTransaction (NoLock) On StatusAlteration.Id=EmployeeTransaction.ReferenceId "
                    + " ) AA ON A.Id=AA.EmployeeId AND A.EmployeeStatus=AA.FromType AND DATEDIFF(DAY,AA.EffectiveDate, A.EndEffectiveDate)>= 30 "
                    + " WHERE  "
                    + " (A.EmployeeStatus={1} OR ISNULL(A.EmployeeStatus,{0})={0}) "
                    + " AND  "
                    + " A.Id NOT IN (SELECT itemid FROM _CustomFieldData (nolock) WHERE [Disable Email Reminder] = 1 ) "
                    + " AND  "
                    + " DATEDIFF(DAY,GETDATE(),A.EndEffectiveDate) IN (SELECT Day FROM ESS_SystemInfoDashBoardContractProbation (nolock))  "
                    + this.GetSelectQueryValidEmployee("A", true);
                CmdText = string.Format(CmdText, Convert.ToInt32(EmployeeStatus.Probation), Convert.ToInt32(EmployeeStatus.Contract),
                Convert.ToInt32(_SystemInfo.AlertMinus), Convert.ToInt32(_SystemInfo.AlertPlus));

                DataTable oDataTableProbationContract = this.ExecuteDataTable(CmdText);
                if (oDataTableProbationContract.Rows.Count > 0)
                {
                    CmdText1 = " SELECT Email, FullName FROM [ESS_SystemInfoDashBoardNotificationAdmin] (nolock) "
                        + " INNER JOIN [_Employee] ON ESS_SystemInfoDashBoardNotificationAdmin.Id = _Employee.Id "
                        + " WHERE ESS_SystemInfoDashBoardNotificationAdmin.SourceType = {0} ";
                    CmdText1 = string.Format(CmdText1, (int)SourceTypeESS.AlertContractProbation);
                    DataTable oDataTableAdmin = ExecuteDataTable(CmdText1);
                    List<string> AdminList = new List<string>();
                    foreach (DataRow oDataRow in oDataTableAdmin.Rows)
                    {
                        string sEmail = ConvertObjectToString(oDataRow["Email"]);
                        AdminList.Add(sEmail);
                    }

                    JobStructureList oJobStructureList = new JobStructureList();
                    foreach (DataRow oDataRow in oDataTableProbationContract.Rows)
                    {
                        Guid EmployeeId = ConvertObjectToGuid(oDataRow["EmployeeId"]);
                        int DayRange = ConvertObjectToInt(oDataRow["DayRange"]);
                        int Gender = ConvertObjectToInt(oDataRow["Gender"]);
                        oJobStructureList = LoadsJobStructure(EmployeeId);

                        string EmployeeName = string.Empty;
                        if (Gender == 0)
                        {
                            EmployeeName += "Sdri. ";
                        }
                        else EmployeeName += "Sdr. ";
                        EmployeeName += oDataRow["EmployeeFullName"].ToString();

                        sMailTemplateId = ConfigurationSettings.AppSettings["EmailToEmployeeNSuperiorForContractProbationAlert"].ToString();

                        if (sMailTemplateId != string.Empty)
                        {
                            DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                            if (oDataTableMailTemplate.Rows.Count > 0)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforContractProbation(sBody, EmployeeName);
                                    //LocalSendMail(_SMTPUsername, drAdmin["Email"].ToString(), sSubject, sBody, null, _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);

                                    if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval1) != null)
                                    {
                                        string Email = string.Empty;
                                        if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval1).EmployeeId != EmployeeId)
                                        {
                                            if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2) != null)
                                            {
                                                if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("000") || oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("00"))
                                                {
                                                    Email += oJobStructureList.GetNextJob(ApprovalLevel.NextApproval1).EmployeeMail;
                                                }
                                            }
                                        }
                                        if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2) != null)
                                        {
                                            if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).EmployeeId != EmployeeId)
                                            {
                                                if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("000") || oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("00"))
                                                {
                                                    if (Email != string.Empty) Email += ";";
                                                    Email += oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).EmployeeMail;
                                                }
                                            }
                                        }
                                        if (Email != string.Empty)
                                        {

                                            //this.SendMail(_MailInfo.AdminEmail, Email, oMailTemplate.Subject, oMailTemplate.Body, AdminList.ToArray(), _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                            LocalSendMail(_SMTPUsername, Email, sSubject, sBody, AdminList.ToArray(), _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                        }
                                    }

                                }
                            }
                        }
                    }

                }
                #endregion

                #region alert probation/contract after 2 month
                CmdText = " SELECT "
                    + " A.*, _Employee.FullName AS EmployeeFullName, _Employee.Email as EmployeeEmail "
                    + " FROM EmployeeTransaction A (nolock) "
                    + " INNER JOIN _Employee (nolock) ON A.EmployeeId = _Employee.Id "
                    + " WHERE A.ReferenceId = '" + ConfigurationSettings.AppSettings["AlertContractProbationRefId"].ToString() + "'";
                DataTable oDataTableContract = this.ExecuteDataTable(CmdText);

                CmdText = " SELECT A.Id AS EmployeeId, A.Gender,A.Number As \"Employee ID\", A.FullName AS \"EmployeeFullname\", A.Email AS EmployeeEmail, "
                    + " A.JoinDate AS JoinDate, Datediff(DAY,GETDATE(),A.EndEffectiveDate) AS DayRange, "
                    + " dbo.fn_EmployeeStatus(A.EmployeeStatus) AS EmployeeStatus, A.EmployeeStatus AS EmployeeStatusId, A.Id, "
                    + " A.EndEffectiveDate AS \"End Contract/Probation/Expire Date\", "
                    //+ " AA.ID AS TransactionId, AA.Number AS RegNo, "
                    + " 'Status Alteration' AS Type, " + Convert.ToInt32(SourceType.StatusAlteration) + " AS TypeId "
                    + "  FROM _Employee A (NoLock) "
                    //+ " LEFT JOIN ( "
                    //+ " SELECT Number, EmployeeId, EffectiveDate, FromType, StatusAlteration.ID  FROM StatusAlteration (NoLock) "
                    //+ " INNER JOIN EmployeeTransaction (NoLock) On StatusAlteration.Id=EmployeeTransaction.ReferenceId "
                    //+ " ) AA ON A.Id=AA.EmployeeId AND A.EmployeeStatus=AA.FromType AND DATEDIFF(DAY,AA.EffectiveDate, A.EndEffectiveDate)>= 30 "
                    + " WHERE  "
                    + " (A.EmployeeStatus={1} OR ISNULL(A.EmployeeStatus,{0})={0}) "
                    + " AND  "
                    + " A.Id NOT IN (SELECT itemid FROM _CustomFieldData (nolock) WHERE [Disable Email Reminder] = 1 ) "
                    + " AND  "
                    + " DATEDIFF(DAY,A.JoinDate,GETDATE()) = " + ConfigurationSettings.AppSettings["AlertContractAfter2Month"].ToString()
                    + this.GetSelectQueryValidEmployee("A", true);
                CmdText = string.Format(CmdText, Convert.ToInt32(EmployeeStatus.Probation), Convert.ToInt32(EmployeeStatus.Contract),
                Convert.ToInt32(_SystemInfo.AlertMinus), Convert.ToInt32(_SystemInfo.AlertPlus));

                DataTable oDataTableProbationContract2 = this.ExecuteDataTable(CmdText);
                if (oDataTableProbationContract2.Rows.Count > 0)
                {
                    CmdText1 = " SELECT Email, FullName FROM [ESS_SystemInfoDashBoardNotificationAdmin] (nolock) "
                        + " INNER JOIN [_Employee] ON ESS_SystemInfoDashBoardNotificationAdmin.Id = _Employee.Id "
                        + " WHERE ESS_SystemInfoDashBoardNotificationAdmin.SourceType = {0} ";
                    CmdText1 = string.Format(CmdText1, (int)SourceTypeESS.AlertContractProbation);
                    DataTable oDataTableAdmin = ExecuteDataTable(CmdText1);
                    List<string> AdminList = new List<string>();
                    foreach (DataRow oDataRow in oDataTableAdmin.Rows)
                    {
                        string sEmail = ConvertObjectToString(oDataRow["Email"]);
                        AdminList.Add(sEmail);
                    }

                    JobStructureList oJobStructureList = new JobStructureList();
                    foreach (DataRow oDataRow in oDataTableProbationContract2.Rows)
                    {
                        Guid EmployeeId = ConvertObjectToGuid(oDataRow["EmployeeId"]);
                        int DayRange = ConvertObjectToInt(oDataRow["DayRange"]);
                        int Gender = ConvertObjectToInt(oDataRow["Gender"]);
                        string sEmployeeEmail = ConvertObjectToString(oDataRow["EmployeeEmail"]);
                        oJobStructureList = LoadsJobStructure(EmployeeId);

                        string EmployeeName = string.Empty;
                        if (Gender == 0)
                        {
                            EmployeeName += "Sdri. ";
                        }
                        else EmployeeName += "Sdr. ";
                        EmployeeName += oDataRow["EmployeeFullName"].ToString();

                        #region email ke atasan dan admin
                        sMailTemplateId = ConfigurationSettings.AppSettings["EmailToEmployeeNSuperiorForContractProbationAlert2"].ToString();
                        if (sMailTemplateId != string.Empty)
                        {
                            DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                            if (oDataTableMailTemplate.Rows.Count > 0)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforContractProbation(sBody, EmployeeName);

                                    if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval1) != null)
                                    {
                                        string Email = string.Empty;
                                        if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval1).EmployeeId != EmployeeId)
                                        {
                                            if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2) != null)
                                            {
                                                if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("000") || oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("00"))
                                                {
                                                    Email += oJobStructureList.GetNextJob(ApprovalLevel.NextApproval1).EmployeeMail;
                                                }
                                            }
                                        }
                                        if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2) != null)
                                        {
                                            if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).EmployeeId != EmployeeId)
                                            {
                                                if (oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("000") || oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).JobName.EndsWith("00"))
                                                {
                                                    if (Email != string.Empty) Email += ";";
                                                    Email += oJobStructureList.GetNextJob(ApprovalLevel.NextApproval2).EmployeeMail;
                                                }
                                            }
                                        }
                                        if (Email != string.Empty)
                                        {
                                            LocalSendMail(_SMTPUsername, Email, sSubject, sBody, AdminList.ToArray(), _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                        }
                                    }

                                }
                            }
                        }
                        #endregion

                        #region email ke karyawan
                        sMailTemplateId = ConfigurationSettings.AppSettings["EmailToEmployeeNSuperiorForContractProbationAlert2B"].ToString();
                        if (sMailTemplateId != string.Empty)
                        {
                            DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                            if (oDataTableMailTemplate.Rows.Count > 0)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforContractProbation(sBody, EmployeeName);

                                    if (sEmployeeEmail != string.Empty)
                                    {
                                        LocalSendMail(_SMTPUsername, sEmployeeEmail, sSubject, sBody, null, _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                }
                #endregion

                #region masa bakti
                CmdText = " SELECT DATEDIFF(YEAR,DateOfBirth,GETDATE()),DateOfBirth,EmployeeFullName, "
                        + " JobDescription,DepartmentDescription,JoinDate,DATEDIFF(YEAR,JoinDate,GETDATE()) AS [MasaKerja] "
                        + "  FROM vEmployee (nolock) "
                        + " WHERE status!=4 AND ResignDate IS NULL AND DATEDIFF(MONTH,JoinDate,GETDATE()) != 0"
                        + " AND DATEDIFF(MONTH,JoinDate,GETDATE()) % (5*12) = 0  "
                        + " ORDER BY DATEDIFF(YEAR,JoinDate,GETDATE()) DESC ";
                DataTable oDataTableMasaBakti = ExecuteDataTable(CmdText);
                if (oDataTableMasaBakti.Rows.Count > 0 && GetDateServer().Day == 1)
                {
                    CmdText1 = " SELECT Email, FullName FROM [ESS_SystemInfoDashBoardNotificationAdmin] (nolock) "
                        + " INNER JOIN [_Employee] ON ESS_SystemInfoDashBoardNotificationAdmin.Id = _Employee.Id "
                        + " WHERE ESS_SystemInfoDashBoardNotificationAdmin.SourceType = {0} ";
                    CmdText1 = string.Format(CmdText1, (int)SourceTypeESS.AlertMasaBakti);
                    DataTable oDataTableAdmin = ExecuteDataTable(CmdText1);

                    //string sContent = "Berikut kami sampaikan nama karyawan yang telah memiliki masa bakti pada " + GetDateServer().ToString("MMM yyyy") + " : ";
                    sContent = "<BR/><BR/><Table border=\"1\" cellspacing=\"0\" cellpadding=\"3\">"
                        + " <tr><td><FONT face=Tahoma size=2>NO.</Font></td><td><FONT face=Tahoma size=2>NAMA</Font></td> "
                        + " <td><FONT face=Tahoma size=2>JABATAN</Font></td>"
                        + " <td><FONT face=Tahoma size=2>DIVISI</Font></td>"
                        + " <td><FONT face=Tahoma size=2>JOIN DATE</Font></td>"
                        + " <td><FONT face=Tahoma size=2>MASA KERJA</Font></td>"
                        + " </tr>";

                    int i = 1;
                    foreach (DataRow oDataRow in oDataTableMasaBakti.Rows)
                    {
                        sContent += " <tr><td><FONT face=Tahoma size=2>" + i.ToString() + "</Font></td><td><FONT face=Tahoma size=2>" + oDataRow["EmployeeFullName"].ToString() + "</Font></td> "
                        + " <td><FONT face=Tahoma size=2>" + oDataRow["JobDescription"].ToString() + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + oDataRow["DepartmentDescription"].ToString() + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + ConvertObjectToDateTime(oDataRow["JoinDate"]).ToString("dd MMM yyyy") + "</Font></td>"
                        + " <td><FONT face=Tahoma size=2>" + oDataRow["MasaKerja"].ToString() + "</Font></td>"
                        + " </tr>";
                        i++;
                    }
                    sContent += "</Table>";

                    sMailTemplateId = ConfigurationSettings.AppSettings["EmailToAdminForMasaBaktiListAlert"].ToString();

                    if (sMailTemplateId != string.Empty)
                    {
                        foreach (DataRow drAdmin in oDataTableAdmin.Rows)
                        {
                            DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                            if (oDataTableMailTemplate.Rows.Count > 0)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforPensionNContractProbation(sBody, drAdmin["FullName"].ToString(), GetDateServer().Year.ToString(), sContent);
                                    LocalSendMail(_SMTPUsername, drAdmin["Email"].ToString(), sSubject, sBody, null, _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                }
                            }
                        }
                    }

                }

                #endregion

                #region ultah
                CmdText = " SELECT FullName, DateOfBirth, Email, DATEDIFF(YEAR,DateOfBirth,GETDATE()) AS Umur  "
                    + " FROM _Employee WHERE ResignDate IS NULL AND ISNULL(Email, '') <> '' "
                    + " AND DAY(DateOfBirth) = DAY(GETDATE()) AND MONTH(DateOfBirth) = MONTH(GETDATE()) AND DATEDIFF(YEAR,DateOfBirth,GETDATE())>0 ";
                DataTable oDataTableHBD = ExecuteDataTable(CmdText);
                if (oDataTableHBD.Rows.Count > 0)
                {
                    
                    if (sMailTemplateId != string.Empty)
                    {
                        DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                        if (oDataTableMailTemplate.Rows.Count > 0)
                        {
                            foreach (DataRow oDataRow in oDataTableHBD.Rows)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforHBDNAniversary(sBody, oDataRow["FullName"].ToString(), oDataRow["Umur"].ToString(), ConvertObjectToDateTime(oDataRow["DateOfBirth"]));
                                    LocalSendMail(_SMTPUsername, oDataRow["Email"].ToString(), sSubject, sBody, null, _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region anniversary
                CmdText = " SELECT FullName, JoinDate, Email, DATEDIFF(YEAR,JoinDate,GETDATE()) AS Umur  "
                    + " FROM _Employee WHERE ResignDate IS NULL AND ISNULL(Email, '') <> '' "
                    + " AND DAY(JoinDate) = DAY(GETDATE()) AND MONTH(JoinDate) = MONTH(GETDATE()) AND DATEDIFF(YEAR,JoinDate,GETDATE())>0";
                DataTable oDataTableAnniversary = ExecuteDataTable(CmdText);
                if (oDataTableAnniversary.Rows.Count > 0)
                {
                    sMailTemplateId = ConfigurationSettings.AppSettings["EmailToEmployeeForAnniversary"].ToString();

                    if (sMailTemplateId != string.Empty)
                    {
                        DataTable oDataTableMailTemplate = LoadsMailNotificationTemplate();
                        if (oDataTableMailTemplate.Rows.Count > 0)
                        {
                            foreach (DataRow oDataRow in oDataTableAnniversary.Rows)
                            {
                                DataRow[] oDataRowList = oDataTableMailTemplate.Select("Id='" + sMailTemplateId.ToString() + "'");
                                if (oDataRowList.Length > 0)
                                {
                                    string sSubject = oDataRowList[0]["Subject"].ToString();
                                    string sBody = oDataRowList[0]["Body"].ToString();
                                    sBody = ReplaceMailBodyforHBDNAniversary(sBody, oDataRow["FullName"].ToString(), oDataRow["Umur"].ToString(), ConvertObjectToDateTime(oDataRow["JoinDate"]));
                                    LocalSendMail(_SMTPUsername, oDataRow["Email"].ToString(), sSubject, sBody, null, _SMTPServer, _SMTPAuthentication, _SMTPUsername, _SMTPPassword);
                                }
                            }
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return true;
        }

        private void LocalSendMail(object args)
        {
            Array argArray = new object[3];
            argArray = (Array)args;
            string From = (string)argArray.GetValue(0);
            string To = (string)argArray.GetValue(1);
            string Subject = (string)argArray.GetValue(2);
            string Body = (string)argArray.GetValue(3);
            string[] CC = (string[])argArray.GetValue(4);
            string CcString = string.Empty;
            string SMTPServer = (string)argArray.GetValue(5);
            string SMTPAuthentication = (string)argArray.GetValue(6);
            string SMTPUsername = (string)argArray.GetValue(7);
            string SMTPPassword = (string)argArray.GetValue(8);
            string Type = (string)argArray.GetValue(9);

            try
            {
                //DotNetNuke.Services.Mail.Mail.SendMail(From, To, string.Empty, Subject, Body, string.Empty, Type, SMTPServer, SMTPAuthentication, SMTPUsername, SMTPPassword);

                if (CC != null)
                {
                    if (CC.Length > 0)
                    {
                        int i = 0;
                        foreach (string sCC in CC)
                        {
                            ++i;
                            if (i == CC.Length)
                            {
                                CcString += sCC;
                            }
                            else
                            {
                                CcString += sCC + "; ";
                            }
                            
                        }
                    }
                }

                
                this.SendMail(From, To, CcString, "", "", MailPriority.Normal, Subject, DotNetNuke.Services.Mail.MailFormat.Html, Encoding.Default,
                                      Body, null, SMTPServer, SMTPAuthentication, SMTPUsername, SMTPPassword, true);

                //Mail.SendMail(From, To, CcString, string.Empty, MailPriority.Normal, Subject, MailFormat.Html, Encoding.Default, Body, string.Empty, SMTPServer, SMTPAuthentication, SMTPUsername, SMTPPassword);
                ////Mail.SendMail(From, To, string.Empty, Subject, Body, string.Empty, "html", _SMTPUsername, SMTPAuthentication, SMTPUsername, SMTPPassword);
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }
            finally
            {

            }
        }

        private bool LocalSendMail(string From, string To,
            string Subject, string Body, string[] CC, string SMTPServer, string SMTPAuthentication,
            string SMTPUsername, string SMTPPassword)
        {
            object args = new object[10] { From, To,
             Subject,  Body, CC, SMTPServer,  SMTPAuthentication,
             SMTPUsername,  SMTPPassword, "html" };

            Thread oThread = new Thread(new ParameterizedThreadStart(LocalSendMail));
            oThread.Start(args);
            return true;
        }

        private string ReplaceMailBodyforPensionNContractProbation(string sMailBody, string sRequesterName, string sYear, string sTransactionSummary)
        {
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.RequesterName.ToString() + "#", sRequesterName);
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.Comment.ToString() + "#", sYear);
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.TransactionSummary.ToString() + "#", sTransactionSummary);

            return sMailBody;
        }

        private string ReplaceMailBodyforContractProbation(string sMailBody, string sRequesterName)
        {
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.RequesterName.ToString() + "#", sRequesterName);
            return sMailBody;
        }

        private string ReplaceMailBodyforHBDNAniversary(string sMailBody, string sRequesterName, string sAge, DateTime dBirthOfDate)
        {
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.RequesterName.ToString() + "#", sRequesterName);
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.Comment.ToString() + "#", sAge);

            DateTime dTemp = new DateTime(DateTime.Now.Year, dBirthOfDate.Month, dBirthOfDate.Day);
            sMailBody = sMailBody.Replace("#" + MailTransactionContent.RequestedDate.ToString() + "#", dTemp.ToString("dd MMM yyyy"));
            return sMailBody;
        }
    }
}
