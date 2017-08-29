using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andal.CommonLibrary;

namespace Andal.Reminder.Services
{
    public enum ApprovalLevel
    {
        [StringValueAttribute("Start")]
        Start = 0,
        [StringValueAttribute("Next Approval 1")]
        NextApproval1 = 1,
        [StringValueAttribute("Next Approval 2")]
        NextApproval2 = 2,
        [StringValueAttribute("Next Approval 3")]
        NextApproval3 = 3,
        [StringValueAttribute("Next Approval 4")]
        NextApproval4 = 4,
        [StringValueAttribute("Next Approval 5")]
        NextApproval5 = 5,
        [StringValueAttribute("Next Approval 6")]
        NextApproval6 = 6,
        [StringValueAttribute("Next Approval 7")]
        NextApproval7 = 7,
        [StringValueAttribute("Next Approval 8")]
        NextApproval8 = 8,
        [StringValueAttribute("Job Position 1")]
        JobPosition1 = 11,
        [StringValueAttribute("Job Position 2")]
        JobPosition2 = 12,
        [StringValueAttribute("Job Position 3")]
        JobPosition3 = 13,
        [StringValueAttribute("Job Position 4")]
        JobPosition4 = 14,
        [StringValueAttribute("Job Position 5")]
        JobPosition5 = 15,
        [StringValueAttribute("Job Position 6")]
        JobPosition6 = 16,
        [StringValueAttribute("Job Position 7")]
        JobPosition7 = 17,
        [StringValueAttribute("Job Position 8")]
        JobPosition8 = 18,
        [StringValueAttribute("Admin 1")]
        Admin1 = 21,
        [StringValueAttribute("Admin 2")]
        Admin2 = 22,
        [StringValueAttribute("Admin 3")]
        Admin3 = 23,
        [StringValueAttribute("Admin 4")]
        Admin4 = 24,
        [StringValueAttribute("Admin 5")]
        Admin5 = 25,
        [StringValueAttribute("End")]
        End = 1000
    }

    [Serializable]
    public class JobStructureList : List<JobStructure>
    {
        private bool _SkipEmpty = false;

        public bool SkipEmpty
        {
            get { return _SkipEmpty; }
            set { _SkipEmpty = value; }
        }


        public JobStructure IsExists(Guid JobId)
        {
            foreach (JobStructure oJobStructure in this)
            {
                if (oJobStructure.JobId == JobId) return oJobStructure;
            }
            return null;
        }

        public JobStructure First
        {
            get
            {
                JobStructure oJobStructure = null;
                foreach (JobStructure oJobStructureTemp in this)
                {
                    if (oJobStructure == null) oJobStructure = oJobStructureTemp;
                    else if (oJobStructure.JobLevel < oJobStructureTemp.JobLevel) oJobStructure = oJobStructureTemp;
                }
                return oJobStructure;
            }
        }

        public JobStructure Next(JobStructure oJobStructure)
        {
            while (oJobStructure.JobParentId != Guid.Empty)
            {
                foreach (JobStructure oJobStructureTemp in this)
                {
                    if (oJobStructureTemp.JobId == oJobStructure.JobParentId)
                    {
                        if (SkipEmpty && oJobStructureTemp.EmployeeId == Guid.Empty) oJobStructure = oJobStructureTemp;
                        else return oJobStructureTemp;
                    }
                }
            }
            return null;
        }

        public JobStructure GetLevel(ApprovalLevel oApprovalLevel)
        {
            int i = 0;
            switch (oApprovalLevel)
            {
                case ApprovalLevel.JobPosition1: i = 1; break;
                case ApprovalLevel.JobPosition2: i = 2; break;
                case ApprovalLevel.JobPosition3: i = 3; break;
                case ApprovalLevel.JobPosition4: i = 4; break;
                case ApprovalLevel.JobPosition5: i = 5; break;
                case ApprovalLevel.JobPosition6: i = 6; break;
                case ApprovalLevel.JobPosition7: i = 7; break;
                case ApprovalLevel.JobPosition8: i = 8; break;
            }
            foreach (JobStructure oJobStructure in this)
            {
                if (oJobStructure.JobLevel == i) return oJobStructure;
            }
            return null;
        }

        public JobStructure GetNextJob(ApprovalLevel oApprovalLevel)
        {
            int i = 0;
            switch (oApprovalLevel)
            {
                case ApprovalLevel.NextApproval1: i = 1; break;
                case ApprovalLevel.NextApproval2: i = 2; break;
                case ApprovalLevel.NextApproval3: i = 3; break;
                case ApprovalLevel.NextApproval4: i = 4; break;
                case ApprovalLevel.NextApproval5: i = 5; break;
                case ApprovalLevel.NextApproval6: i = 6; break;
                case ApprovalLevel.NextApproval7: i = 7; break;
                case ApprovalLevel.NextApproval8: i = 8; break;
            }

            JobStructure oJobStructure = this.First;
            for (int loop = 0; loop < i; loop++)
            {
                if (oJobStructure == null) return null;
                else oJobStructure = this.Next(oJobStructure);
            }
            return oJobStructure;
        }

    }

    [Serializable]
    public class JobStructure
    {
        private Guid _EmployeeId = Guid.Empty;
        private Guid _JobId = Guid.Empty;
        private Guid _JobParentId = Guid.Empty;
        private string _JobName = string.Empty;
        private string _JobDescription = string.Empty;
        private string _EmployeeName = string.Empty;
        private string _EmployeeNumber = string.Empty;
        private string _EmployeeMail = string.Empty;
        private int _JobLevel = 0;

        public string EmployeeMail
        {
            get { return _EmployeeMail; }
            set { _EmployeeMail = value; }
        }

        public int JobLevel
        {
            get { return _JobLevel; }
            set { _JobLevel = value; }
        }

        public Guid EmployeeId
        {
            get { return _EmployeeId; }
            set { _EmployeeId = value; }
        }

        public Guid JobId
        {
            get { return _JobId; }
            set { _JobId = value; }
        }

        public Guid JobParentId
        {
            get { return _JobParentId; }
            set { _JobParentId = value; }
        }

        public string JobName
        {
            get { return _JobName; }
            set { _JobName = value; }
        }

        public string JobDescription
        {
            get { return _JobDescription; }
            set { _JobDescription = value; }
        }

        public string EmployeeName
        {
            get { return _EmployeeName; }
            set { _EmployeeName = value; }
        }

        public string EmployeeNumber
        {
            get { return _EmployeeNumber; }
            set { _EmployeeNumber = value; }
        }

        public JobStructure()
        {

        }
    }
}
