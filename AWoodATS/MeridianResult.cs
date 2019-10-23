using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AWoodATS
{
    public class MeridianResult
    {
        public MeridianResult()
        {
            IsSchedule = true;
            UniqueID = string.Empty;
            UploadFromLocalPath = false;
            Approve01 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Approve02 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Approve03 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Approve04 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Approve05 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Pending01 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Pending02 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Pending03 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Pending04 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Pending05 = MeridianGlobalConstants.XCBL_NO_FLAG;
            Rejected01 = MeridianGlobalConstants.XCBL_NO_FLAG;
        }

        public string Status { get; set; }
        public bool IsSchedule { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; internal set; }
        public string FtpServerInFolderPath { get; set; }
        public string FtpServerOutFolderPath { get; set; }
        public string LocalFilePath { get; set; }
        public string WebUserName { get; set; }
        public string WebPassword { get; set; }
        public string WebHashKey { get; set; }
        public string UniqueID { get; set; }
        public string OrderNumber { get; set; }
        public string FileName { get; set; }
        public bool UploadFromLocalPath { get; set; }
        public byte[] Content { get; set; }
        public XmlDocument XmlDocument { get; set; }
        public string Approve01 { get; set; }
        public string Approve02 { get; set; }
        public string Approve03 { get; set; }
        public string Approve04 { get; set; }
        public string Approve05 { get; set; }
        public string Pending01 { get; set; }
        public string Pending02 { get; set; }
        public string Pending03 { get; set; }
        public string Pending04 { get; set; }
        public string Pending05 { get; set; }
        public string Rejected01 { get; set; }
        public string Comments { get; set; }
    }
}