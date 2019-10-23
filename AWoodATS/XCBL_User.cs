using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AWoodATS
{
    public class XCBL_User
    {
        /// <summary>
        /// The xCBL Web Service Username 
        /// </summary>
        public string WebUsername { get; set; }

        /// <summary>
        /// The xCBL Web Service Password
        /// </summary>
        public string WebPassword { get; set; }

        /// <summary>
        /// The xCBL Web Service Hashkey for the User
        /// </summary>
        public string Hashkey { get; set; }

        /// <summary>
        /// The FTP Username to upload CSV files
        /// </summary>
        public string FtpUsername { get; set; }

        /// <summary>
        /// The FTP Password to upload CSV files
        /// </summary>
        public string FtpPassword { get; set; }

        /// <summary>
        /// The FTP Server In Folder Path
        /// </summary>
        public string FtpServerInFolderPath { get; set; }

        /// <summary>
        /// The FTP Server Out Folder Path
        /// </summary>
        public string FtpServerOutFolderPath { get; set; }

        /// <summary>
        /// The Contact Name for the Web Service to contact if an issue is encountered
        /// </summary>
        public string WebContactName { get; set; }

        /// <summary>
        /// The Company name of the Contact 
        /// </summary>
        public string WebContactCompany { get; set; }

        /// <summary>
        /// The Email address of the Contact
        /// </summary>
        public string WebContactEmail { get; set; }

        /// <summary>
        /// The first Phone Number option of the Contact
        /// </summary>
        public string WebContactPhone1 { get; set; }

        /// <summary>
        /// The second Phone Number option of the contact
        /// </summary>
        public string WebContactPhone2 { get; set; }

        /// <summary>
        /// If the user record is enabled or disabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The Local File Path
        /// </summary>
        public string LocalFilePath { get; set; }
    }
}