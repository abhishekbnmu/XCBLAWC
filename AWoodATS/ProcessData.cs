using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AWoodATS
{
    public class ProcessData
    {
        /// <summary>
        /// The xCBL Web Service Username 
        /// </summary>
        public string WebUserName { get; set; }

        /// <summary>
        /// The FTP Username to upload CSV files
        /// </summary>
        public string FtpUserName { get; set; }
        public string ScheduleID { get; set; } /*This is specific for Shipping Schedule Request*/
        public string RequisitionID { get; set; } /*This is specific for Requisition Request*/
        public string ScheduleResponseID { get; set; } /*This is specific for Shipping Schedule Response Request*/
        public string OrderNumber { get; set; }
        public string CsvFileName { get; set; }
        public string XmlFileName { get; set; }
        public ShippingSchedule ShippingSchedule { get; set; }
       // public Requisition Requisition { get; set; }
        public ShippingScheduleResponse ShippingScheduleResponse { get; set; }
        public XmlDocument XmlDocument { get; set; }
        public string FtpPassword { get; internal set; }
        public string FtpServerInFolderPath { get; internal set; }
        public string FtpServerOutFolderPath { get; internal set; }
        public string LocalFilePath { get; internal set; }
    }
}