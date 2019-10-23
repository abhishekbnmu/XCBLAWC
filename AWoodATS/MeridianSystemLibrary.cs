using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Data.SqlClient;
namespace AWoodATS
{
    public static partial class MeridianSystemLibrary
    {
        public static string ReplaceSpecialCharsWithSpace(this string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    char charLineFeed = (char)10;
                    char charCarriageReturn = (char)13;
                    char charComma = (char)44;

                    if (value.IndexOf(charCarriageReturn) != -1)
                        value = value.Replace(charCarriageReturn.ToString(), " ");

                    if (value.IndexOf(charLineFeed) != -1)
                        value = value.Replace(charLineFeed.ToString(), " ");

                    if (value.IndexOf(charComma) != -1)
                        value = value.Replace(charComma.ToString(), " ");
                }
                return value;
            }
            catch
            {
                return value;
            }
        }

        public static string GetMeridian_Status(string status, string uniqueId, bool isShippingSchedule = true)
        {
            StringBuilder messageResponse = new StringBuilder();
            messageResponse.AppendLine(MeridianGlobalConstants.XML_HEADER);
            messageResponse.AppendLine(isShippingSchedule ? MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_OPEN_TAG : MeridianGlobalConstants.MESSAGE_REQUISITION_ACKNOWLEDGEMENT_OPEN_TAG);
            messageResponse.AppendLine(string.Format(MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_REFERENCE_NUMBER_OPEN_TAG + "{0}" + MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_REFERENCE_NUMBER_CLOSE_TAG, uniqueId));
            messageResponse.AppendLine(string.Format(MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_NOTE_OPEN_TAG + "{0}" + MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_NOTE_CLOSE_TAG, status));
            messageResponse.AppendLine(MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_CLOSE_TAG);
            return messageResponse.ToString();
        }

        public static XmlNode GetNodeByNameAndInnerTextLogWarningTrans(this XmlNode fromNode, XmlNamespaceManager nsMgr, string nodeName, string warningNumber, ProcessData processData, string uniqueId, string methodName = "")
        {
            try
            {
                XmlNode foundNode = fromNode.SelectSingleNode(nodeName, nsMgr);
                //if (foundNode == null || string.IsNullOrEmpty(foundNode.InnerText))
                 //   LogTransaction(processData.WebUserName, processData.FtpUserName, !string.IsNullOrEmpty(methodName) ? methodName : "ValidateScheduleShippingXmlDocument", string.Format("02.{0}", warningNumber), string.Format("Warning - There was an exception retrieving {0} xml node or tag.", nodeName), string.Format("Warning - There was an exception retrieving {0} xml node or tag.", nodeName), processData.CsvFileName, uniqueId, processData.OrderNumber, processData.XmlDocument, string.Format("Warning {0} - Issue with node {1}", warningNumber, nodeName));
                return foundNode;
            }
            catch (Exception e)
            {
               // LogTransaction(processData.WebUserName, processData.FtpUserName, !string.IsNullOrEmpty(methodName) ? methodName : "ValidateScheduleShippingXmlDocument", string.Format("02.{0}", warningNumber), string.Format("Warning - There was an exception retrieving {0} xml node or tag.", nodeName), Convert.ToString(e.Message), processData.CsvFileName, uniqueId, processData.OrderNumber, processData.XmlDocument, string.Format("Warning {0} - Issue with node {1}", warningNumber, nodeName));
                return null;
            }
        }
        public static XmlNode GetNodeByNameAndLogErrorTrans(this XmlNode fromNode, XmlNamespaceManager nsMgr, string nodeName, string errorNumber, ProcessData processData, string uniqueId, string methodName = "")
        {
            try
            {

                XmlNode foundNode = fromNode.SelectSingleNode(nodeName, nsMgr);//
                //if (foundNode == null || string.IsNullOrEmpty(foundNode.InnerText))
                 //   LogTransaction(processData.WebUserName, processData.FtpUserName, !string.IsNullOrEmpty(methodName) ? methodName : "ValidateScheduleShippingXmlDocument", string.Format("03.{0}", errorNumber), string.Format("Error - There was an exception retrieving {0} xml node or tag or empty.", nodeName), string.Format("Error - There was an exception retrieving {0} xml node or tag or empty.", nodeName), processData.CsvFileName, uniqueId, processData.OrderNumber, processData.XmlDocument, string.Format("Error 03.{0} - Issue with node {1}", errorNumber, nodeName));
                return foundNode;
            }
            catch (Exception e)
            {
                //LogTransaction(processData.WebUserName, processData.FtpUserName, !string.IsNullOrEmpty(methodName) ? methodName : "ValidateScheduleShippingXmlDocument", string.Format("03.{0}", errorNumber), string.Format("Error - There was an exception retrieving {0} xml node or tag or empty.", nodeName), Convert.ToString(e.Message), processData.CsvFileName, uniqueId, processData.OrderNumber, processData.XmlDocument, string.Format("Error 03.{0} - Issue with node {1}", errorNumber, nodeName));
                return null;
            }
        }

        public static void SetOtherScheduleReferenceDesc(this ShippingSchedule shippingSchedule, string referenceTypeCodedOther, string referenceDescription)
        {
            string referenceTypeCoded = string.Format("Other_{0}", referenceTypeCodedOther).ToLower().Trim();

            switch (referenceTypeCoded)
            {
                case "other_firststop":
                    shippingSchedule.Other_FirstStop = referenceDescription;
                    break;
                case "other_before7":
                    shippingSchedule.Other_Before7 = referenceDescription;
                    break;
                case "other_before9":
                    shippingSchedule.Other_Before9 = referenceDescription;
                    break;
                case "other_before12":
                    shippingSchedule.Other_Before12 = referenceDescription;
                    break;
                case "other_sameday":
                    shippingSchedule.Other_SameDay = referenceDescription;
                    break;
                case "other_homeowneroccupied":
                    shippingSchedule.Other_OwnerOccupied = referenceDescription;
                    break;
                case "other_workordernumber":
                    shippingSchedule.WorkOrderNumber = referenceDescription;
                    shippingSchedule.Other_7 = referenceDescription;
                    break;
                case "other_ssid":
                    shippingSchedule.SSID = referenceDescription;
                    shippingSchedule.Other_8 = referenceDescription;
                    break;
                case "other_7":
                    shippingSchedule.Other_7 = referenceDescription;
                    break;
                case "other_8":
                    shippingSchedule.Other_8 = referenceDescription;
                    break;
                case "other_9":
                    shippingSchedule.Other_9 = referenceDescription;
                    break;
                case "other_10":
                    shippingSchedule.Other_10 = referenceDescription;
                    break;
                default:
                    break;
            }
        }

        public static XmlNode GetNodeByNameAndLogWarningTrans(this XmlNode fromNode, XmlNamespaceManager nsMgr, string nodeName, string warningNumber, ProcessData processData, string uniqueId, string methodName = "")
        {
            try
            {
                XmlNode foundNode = fromNode.SelectSingleNode(nodeName, nsMgr);
                //if (foundNode == null)
                //    LogTransaction(processData.WebUserName, processData.FtpUserName, !string.IsNullOrEmpty(methodName) ? methodName : "ValidateScheduleShippingXmlDocument", string.Format("02.{0}", warningNumber), string.Format("Warning - There was an exception retrieving {0} xml node or tag.", nodeName), string.Format("Warning - There was an exception retrieving {0} xml node or tag.", nodeName), processData.CsvFileName, uniqueId, processData.OrderNumber, processData.XmlDocument, string.Format("Warning {0} - Issue with node {1}.", "GetNodeByNameAndLogWarningTrans", nodeName));
                return foundNode;
            }
            catch (Exception e)
            {
                //LogTransaction(processData.WebUserName, processData.FtpUserName, !string.IsNullOrEmpty(methodName) ? methodName : "ValidateScheduleShippingXmlDocument", string.Format("02.{0}", warningNumber), string.Format("Warning - There was an exception retrieving {0} xml node or tag.", nodeName), Convert.ToString(e.Message), processData.CsvFileName, uniqueId, processData.OrderNumber, processData.XmlDocument, string.Format("Warning {0} - Issue with node {1}.", "GetNodeByNameAndLogWarningTrans", nodeName));
                return null;
            }
        }
        public static void SetContactNumbers(this ShippingSchedule shippingSchedule, string contactNumber, int contactNumberIndex)
        {
            switch (contactNumberIndex)
            {
                case 0:
                    shippingSchedule.ContactNumber_1 = contactNumber;
                    break;
                case 1:
                    shippingSchedule.ContactNumber_2 = contactNumber;
                    break;
                case 2:
                    shippingSchedule.ContactNumber_3 = contactNumber;
                    break;
                case 3:
                    shippingSchedule.ContactNumber_4 = contactNumber;
                    break;
                case 4:
                    shippingSchedule.ContactNumber_5 = contactNumber;
                    break;
                case 5:
                    shippingSchedule.ContactNumber_6 = contactNumber;
                    break;
                default:
                    break;
            }
        }

        public static void logErrors(Exception ex, string methodName)
        {
            DateTime today = DateTime.Today;
            string FileName = today.ToString("MM-dd-yyyy");
            string appDirectory = Path.GetDirectoryName(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
            string FilePath = "ErrorLog_" + FileName + ".txt";
            string DirPath = Path.Combine(appDirectory, FilePath);
            string lineno = "", exDate = "", exTime = "", exmsg = "", exImsg = "", exST = "";

            bool Flag = false;

            if (!File.Exists(DirPath))
            {
                File.Create(FilePath).Close();
                Flag = true;
            }

            if (Flag == true)
            {
                using (StreamWriter writer = new StreamWriter(DirPath, true))
                {
                    writer.WriteLine("Exception Date ================ Exception Time  ================ Exception Message   ================ Exception StackTrace");
                }
            }
            using (StreamWriter writer = new StreamWriter(DirPath, true))
            {
                const string lineSearch = ":line ";
                var index = ex.StackTrace.LastIndexOf(lineSearch);
                if (index != -1)
                {
                    lineno = lineno + ex.StackTrace.Substring(index + lineSearch.Length);
                }

                exDate = DateTime.Now.ToShortDateString();
                exTime = DateTime.Now.ToShortTimeString();
                exmsg = Convert.ToString(ex.Message);
                exImsg = Convert.ToString(ex.InnerException);
                exST = Convert.ToString(ex.StackTrace);

                // writer.WriteLine(DateTime.Now.ToShortDateString().PadRight(20, c) + DateTime.Now.ToShortTimeString().PadRight(20, c) + ex.Message.ToString().PadRight(20, c) + ex.StackTrace.ToString());
                writer.WriteLine(" ");
                writer.WriteLine("Exception Date : " + DateTime.Now.ToShortDateString());
                writer.WriteLine("Exception Time : " + DateTime.Now.ToShortTimeString());
                writer.WriteLine("Exception Message : " + Convert.ToString(ex.Message));
                writer.WriteLine("Inner Exception : " + Convert.ToString(ex.InnerException));
                writer.WriteLine("Exception StackTrace : " + Convert.ToString(ex.StackTrace));
                writer.WriteLine("Method Name : " + methodName);                
                writer.WriteLine("Line No. : " + lineno);

                writer.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------");
            }
        }

        public static XCBL_User sysGetAuthenticationByUsernameAndPassword(string webUsername, string webPassword)
        {

            // If either the username or password are empty then return null for the method
            if (string.IsNullOrEmpty(webUsername) || string.IsNullOrEmpty(webPassword))
                return null;

            // Try to retrieve the authentication record based on the specified username and password
            try
            {
                DataSet dsRecords = new DataSet();

                using (SqlConnection sqlConnection = new SqlConnection(MeridianGlobalConstants.XCBL_DATABASE_SERVER_URL))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("XCBL_SP_GetXcblAuthenticationUser", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.Parameters.Add("@webUsername", SqlDbType.NVarChar).Value = webUsername;
                        sqlCommand.Parameters.Add("@webPassword", SqlDbType.NVarChar).Value = webPassword;

                        // Fill the data adapter with the sql query results
                        using (SqlDataAdapter sdaAdapter = new SqlDataAdapter(sqlCommand))
                        {
                            sdaAdapter.Fill(dsRecords);
                        }
                    }
                }
                // Parse the authentication record to a XCBL_User class object
                XCBL_User user = new XCBL_User()
                {
                    WebUsername = dsRecords.Tables[0].Rows[0]["CALLER"].ToString(),
                    WebPassword = dsRecords.Tables[0].Rows[0]["Password"].ToString(),
                   
                };

                return user;
            }
            catch (Exception ex)
            {
                // If there was an error encountered in retrieving the authentication record then try to insert a record in MER010TransactionLog table to record the issue
                try
                {
                   // LogTransaction(webUsername, "", "sysGetAuthenticationByUsername", "00.00", "Warning - Cannot retrieve record from MER000Authentication table", ex.InnerException.ToString(), "", "", "", new XmlDocument(), "Warning 26 - DB Connection");
                }
                catch
                {
                }
                return null;
            }
        }

        public static int LogPBS(string scheduleId, string orderNumber, string approve01, string approve02, string approve03, string approve04, string approve05, string pending01, string pending02, string pending03, string pending04, string pending05, string requestType, string rejected01, string comment)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(MeridianGlobalConstants.XCBL_DATABASE_SERVER_URL))
                {
                    sqlConnection.Open();

                    // Try to insert the record into the MER010TransactionLog table
                    using (SqlCommand sqlCommand = new SqlCommand(MeridianGlobalConstants.XCBL_SP_InsPBSLog, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.Add("@scheduleId", SqlDbType.NVarChar).Value = scheduleId;
                        sqlCommand.Parameters.Add("@orderNumber", SqlDbType.NVarChar).Value = orderNumber;
                        sqlCommand.Parameters.Add("@approve01", SqlDbType.NVarChar).Value = approve01;
                        sqlCommand.Parameters.Add("@approve02", SqlDbType.NVarChar).Value = approve02;
                        sqlCommand.Parameters.Add("@approve03", SqlDbType.NVarChar).Value = approve03;
                        sqlCommand.Parameters.Add("@approve04", SqlDbType.NVarChar).Value = approve04;
                        sqlCommand.Parameters.Add("@approve05", SqlDbType.NVarChar).Value = approve05;
                        sqlCommand.Parameters.Add("@pending01", SqlDbType.NVarChar).Value = pending01;
                        sqlCommand.Parameters.Add("@pending02", SqlDbType.NVarChar).Value = pending02;
                        sqlCommand.Parameters.Add("@pending03", SqlDbType.NVarChar).Value = pending03;
                        sqlCommand.Parameters.Add("@pending04", SqlDbType.NVarChar).Value = pending04;
                        sqlCommand.Parameters.Add("@pending05", SqlDbType.NVarChar).Value = pending05;
                        sqlCommand.Parameters.Add("@requestType", SqlDbType.NVarChar).Value = requestType;
                        sqlCommand.Parameters.Add("@rejected01", SqlDbType.NVarChar).Value = rejected01;
                        sqlCommand.Parameters.Add("@comment", SqlDbType.NVarChar).Value = comment;
                        return sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static ProcessData GetNewProcessData(this XCBL_User xCblServiceUser)
        {
            var processData = new ProcessData
            {
                ScheduleID = "No Schedule Id",
                RequisitionID = "No Requisition Id",
                ScheduleResponseID = "No Schedule Response Id",
                OrderNumber = "No Order Number",
                CsvFileName = "No FileName",
                XmlFileName = "No FileName",
                ShippingSchedule = new ShippingSchedule(),
               // Requisition = new Requisition(),
                ShippingScheduleResponse = new ShippingScheduleResponse()
                //WebUserName = xCblServiceUser.WebUsername,
                //FtpUserName = xCblServiceUser.FtpUsername
            };
            return processData;
        }

        public static int UpdateDataToLocal(ProcessData processData)
        {
            try
            {
                int result = 0;
                       
                using (SqlConnection sqlConnection = new SqlConnection(MeridianGlobalConstants.XCBL_DATABASE_SERVER_URL))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("IMPORT_AWC_xCBL", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.Parameters.Add("@trackingnum", SqlDbType.NVarChar).Value = processData.ShippingSchedule.OrderNumber;
                        sqlCommand.Parameters.Add("@firststop", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Other_FirstStop;
                        sqlCommand.Parameters.Add("@before7", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Other_Before7;
                        sqlCommand.Parameters.Add("@before9", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Other_Before9;
                        sqlCommand.Parameters.Add("@before12", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Other_Before12;
                        sqlCommand.Parameters.Add("@sameday", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Other_SameDay;
                        sqlCommand.Parameters.Add("@homeowner", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Other_OwnerOccupied;
                        sqlCommand.Parameters.Add("@lat", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Latitude.ToString();
                        sqlCommand.Parameters.Add("@long", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Longitude.ToString();
                        sqlCommand.Parameters.Add("@name", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Name1;
                        sqlCommand.Parameters.Add("@address1", SqlDbType.NVarChar).Value = processData.ShippingSchedule.Street;
                        sqlCommand.Parameters.Add("@address2", SqlDbType.NVarChar).Value = processData.ShippingSchedule.StreetSupplement1;
                        sqlCommand.Parameters.Add("@city", SqlDbType.NVarChar).Value = processData.ShippingSchedule.City;
                        //sqlCommand.Parameters.Add("@state", SqlDbType.NVarChar).Value = webPassword;
                        sqlCommand.Parameters.Add("@zip", SqlDbType.NVarChar).Value = processData.ShippingSchedule.PostalCode;
                        sqlCommand.Parameters.Add("@phone", SqlDbType.NVarChar).Value = processData.ShippingSchedule.ContactNumber_1;
                        sqlCommand.Parameters.Add("@notes", SqlDbType.NVarChar).Value = processData.ShippingSchedule.ShippingInstruction;
                        sqlCommand.Parameters.Add("@contact", SqlDbType.NVarChar).Value = processData.ShippingSchedule.ContactName;
                        // Fill the data adapter with the sql query results
                        //sqlConnection.Open();
                        result= sqlCommand.ExecuteNonQuery();

                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                // If there was an error encountered in retrieving the authentication record then try to insert a record in MER010TransactionLog table to record the issue
                try
                {
                    ApplicationError.logErrors(ex, "UpdateDataToLocal");
                }
                catch
                {
                }
                return 0;
            }
        }

        public static DataSet GetDataByRef(string Ref)
        {
            DataSet ds = new DataSet();
            try
            {
                
                using (SqlConnection sqlConnection = new SqlConnection(MeridianGlobalConstants.XCBL_DATABASE_SERVER_URL))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("XCL_CHEKSTATUS", sqlConnection))
                    {
                        SqlDataAdapter da = new SqlDataAdapter();
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.Add("@REF", SqlDbType.NVarChar).Value = Ref;
                        da.SelectCommand = sqlCommand;
                        da.Fill(ds);
                         
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex, "UpdateDataToLocal");

            }
            return ds;
        }
    }
}