using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace AWoodATS
{
    public class CommonProcess
    {
        /// <summary>
        /// To authenticate request whether it has valid credential to proceed
        /// </summary>
        /// <param name="xCblServiceUser">Service User</param>
        /// <param name="operationContext">Current OperationContext</param>
        /// <returns></returns>
        internal static bool IsAuthenticatedRequest(OperationContext operationContext, ref XCBL_User xCblServiceUser)
        {
            try
            {
                // If a separate namespace is needed for the Credentials tag use the global const CREDENTIAL_NAMESPACE that is commented below
                int index = operationContext.IncomingMessageHeaders.FindHeader("Credentials", "");

                // Retrieve the first soap headers, this should be the Credentials tag
                MessageHeaderInfo messageHeaderInfo = operationContext.IncomingMessageHeaders[index];

                xCblServiceUser = Meridian_AuthenticateUser(operationContext.IncomingMessageHeaders, messageHeaderInfo, index);
                if (xCblServiceUser == null || string.IsNullOrEmpty(xCblServiceUser.WebUsername) )
                {
                    ApplicationError.WriteMessage("UnAuthenticated Request Incorrect Credentials");
                    
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex, "IsAuthenticatedRequest");
                //  MeridianSystemLibrary.LogTransaction("No WebUser", "No FTPUser", "IsAuthenticatedRequest", "03.01", "Error - New SOAP Request not authenticated", "UnAuthenticated Request", "No FileName", "No Schedule ID", "No Order Number", null, "Error 03.01 - Incorrect Credentials");
                return false;
            }
        }

        /// <summary>
        /// This function will authenticate the User with Username and Password
        /// </summary>
        /// <param name="messageHeaders">SOAP MessageHeaders </param>
        /// <param name="messageHeaderInfo">MessageHeaderInfo - Contains the Soap Credential Header</param>
        /// <param name="objXCBLUser">Object - Holds the user related information</param>
        /// <returns></returns>
        private static XCBL_User Meridian_AuthenticateUser(MessageHeaders messageHeaders, MessageHeaderInfo messageHeaderInfo, int index)
        {
            try
            {
                string username = string.Empty;
                string password = string.Empty;
                string hashkey = string.Empty;

                // Retrieve the Credential header information
                // If a separate namespace is needed for the Credentials tag use the global const CREDENTIAL_NAMESPACE that is commented below
                if (messageHeaderInfo.Name == MeridianGlobalConstants.CREDENTIAL_HEADER)// && h.Namespace == MeridianGlobalConstants.CREDENTIAL_NAMESPACE)
                {
                    // read the value of that header
                    XmlReader xr = messageHeaders.GetReaderAtHeader(index);
                    while (xr.Read())
                    {
                        if (xr.IsStartElement())
                            if (xr.Name == MeridianGlobalConstants.CREDENTIAL_USERNAME)
                            {
                                if (xr.Read())
                                    username = xr.Value;
                            }
                            else if (xr.Name == MeridianGlobalConstants.CREDENTIAL_PASSWORD)
                            {
                                if (xr.Read())
                                    password = xr.Value;
                            }
                            else if (xr.Name == MeridianGlobalConstants.CREDENTIAL_HASHKEY)
                            {
                                if (xr.Read())
                                    hashkey = xr.Value;
                            }
                    }
                }

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    //username = Encryption.Decrypt(username, hashkey);
                   // password = Encryption.Decrypt(password, hashkey);
                    return MeridianSystemLibrary.sysGetAuthenticationByUsernameAndPassword(username, password);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// To Delete created file
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <param name="processData">Process Data</param>
        /// <returns></returns>
        internal static bool DeleteFile(string filePath, MeridianResult meridianResult = null)
        {
            bool result = false;
            if (meridianResult == null)
                meridianResult = new MeridianResult();
            try
            {
                if (File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string ext = Path.GetExtension(filePath);
                    File.Delete(filePath);
                    result = true;
                   // MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "DeleteFile", "01.07", string.Format("Success - Deleted CSV file {0} after ftp upload: {0}", fileName), string.Format("Deleted CSV file: {0}", fileName), fileName, meridianResult.UniqueID, meridianResult.OrderNumber, null, "Success");
                }
                result = true;
            }
            catch (Exception ex)
            {
               /// MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "DeleteFile", "03.09", "Error - Delete CSV File", string.Format("Error - While deleting local CSV file: {0} with error {1}", meridianResult.FileName, ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, null, "Error 03.09 - Delete local CSV");
                result = false;
            }
            return result;
        }


        /// <summary>
        /// To Create file if not exist and on catch safer side: if first call created file but on write got issue so deleting that file so that for next createfile call it creates again and close. 
        /// </summary>
        /// <param name="filePath">File Path </param>
        /// <param name="content">Content want to write</param>
        /// <param name="processData">Process Data</param>
        /// <returns></returns>
        internal static bool CreateFile(string content, MeridianResult meridianResult)
        {
            try
            {
                var filePath = meridianResult.LocalFilePath + meridianResult.FileName;
                string fileName = Path.GetFileName(filePath);
                string ext = Path.GetExtension(filePath);
                DeleteFile(filePath, meridianResult);// Safer side 
                File.Create(filePath).Close();
                File.WriteAllText(filePath, content);
                //MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "CreateFile", "01.04", "Success - Created CSV File", "CSV File Created", meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, null, "Success");
                return true;
            }
            catch (Exception ex)
            {
              //  MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "CreateFile", "03.12", "Error - Create CSV File", string.Format("Error - While creating local CSV file: {0} with error {1}", meridianResult.FileName, ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, null, "Error 03.12 - Create local CSV");
                return false;
            }
        }

        /// <summary>
        /// To Create file if not exist and on catch safer side: if first call created file but on write got issue so deleting that file so that for next createfile call it creates again and close. 
        /// </summary>
        /// <param name="filePath">File Path </param>
        /// <param name="content">Content want to write</param>
        /// <returns></returns>
        internal static bool CreateLogFile(string filePath, string content)
        {
            try
            {
                DeleteFile(filePath);// Safer side 
                File.Create(filePath).Close();
                File.WriteAllText(filePath, content);
                return true;
            }
            catch (Exception ex)
            {
               // MeridianSystemLibrary.LogTransaction(null, null, "CreateLogFile", "03.14", "Error - Create PBS Log File", string.Format("Error - While creating PBS log file with error {0}", ex.Message), filePath, null, null, null, "Error 03.14 - Create PBS Log File");
                return false;
            }
        }

        /// <summary>
        /// To send Shipping Schedule Response to AWC. 
        /// </summary>
        /// <param name="filePath">File Path </param>
        /// <param name="content">Content want to write</param>
        /// <param name="processData">Process Data</param>
        /// <returns></returns>
        internal async static void SendShippingScheduleResponse1(MeridianResult meridianResult, string responseTypeCoded = null)
        {
            try
            {
                string shippingScheduleRequestId = null;

                responseTypeCoded = !string.IsNullOrWhiteSpace(responseTypeCoded) ? responseTypeCoded : (meridianResult.Approve01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve02.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve03.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve04.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve05.Equals(MeridianGlobalConstants.XCBL_YES_FLAG)) ?
                    MeridianGlobalConstants.XCBL_RESPONSE_TYPE_CODED_SHIPPING_SCHEDULE_RESPONSE_ACCEPTED :
                    meridianResult.Rejected01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ?
                    MeridianGlobalConstants.XCBL_RESPONSE_TYPE_CODED_SHIPPING_SCHEDULE_RESPONSE_REJECTED :
                    MeridianGlobalConstants.XCBL_RESPONSE_TYPE_CODED_SHIPPING_SCHEDULE_RESPONSE_PENDING;

                var purposeCoded = MeridianGlobalConstants.XCBL_PURPOSE_CODED_SHIPPING_SCHEDULE_RESPONSE;

                for (int i = 0; i < 3; i++)//Loop through 3 times if got response unsuccessful
                {
                    bool isSuccess = false;
                    /*Below code to send Shipping Schedule Request to AWC*/
                    using (var client = new WebClient())
                    {
                        var data = CreateShippingScheduleResponse(meridianResult, responseTypeCoded, purposeCoded, ref shippingScheduleRequestId);
                        var requestData = Encoding.ASCII.GetBytes(data);
                        client.Headers.Add("Content-Type", "text/xml;charset=utf-8");
                        client.Headers.Add("SOAPAction", MeridianGlobalConstants.CONFIG_AWC_ACTION);
                        try
                        {
                            var responseData = await client.UploadDataTaskAsync(new Uri(MeridianGlobalConstants.CONFIG_AWC_ENDPOINT), requestData);
                            var response = Encoding.ASCII.GetString(responseData);
                            isSuccess = GetCurrentShippingScheduleRequestResponse(response, data, shippingScheduleRequestId, meridianResult.OrderNumber);
                        }
                        catch (Exception ex)
                        {
                           // MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "SendShippingScheduleResponse", "06.07", "Error - Send ShippingScheduleResponse Request", string.Format("Error - While sending SSR Request: {0}", ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, meridianResult.XmlDocument, "Error 06.07 - Send AWC SSR Request");
                        }
                        finally
                        {
                            client.Dispose();
                        }
                    }
                    if (isSuccess)
                        break;
                }

            }
            catch (Exception ex)
            {
                //MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "SendShippingScheduleResponse", "06.07", "Error - Send ShippingScheduleResponse Request", string.Format("Error - While sending SSR Request: {0}", ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, meridianResult.XmlDocument, "Error 06.07 - Send AWC SSR Request");
            }
        }

        /// <summary>
        /// To send Shipping Schedule Response to AWC. 
        /// </summary>
        /// <param name="filePath">File Path </param>
        /// <param name="content">Content want to write</param>
        /// <param name="processData">Process Data</param>
        /// <returns></returns>
        internal static void SendShippingScheduleResponse(MeridianResult meridianResult, string responseTypeCoded = null)
        {
            try
            {
                string shippingScheduleRequestId = null;

                responseTypeCoded = !string.IsNullOrWhiteSpace(responseTypeCoded) ? responseTypeCoded : (meridianResult.Approve01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve02.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve03.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve04.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                    meridianResult.Approve05.Equals(MeridianGlobalConstants.XCBL_YES_FLAG)) ?
                    MeridianGlobalConstants.XCBL_RESPONSE_TYPE_CODED_SHIPPING_SCHEDULE_RESPONSE_ACCEPTED :
                    meridianResult.Rejected01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ?
                    MeridianGlobalConstants.XCBL_RESPONSE_TYPE_CODED_SHIPPING_SCHEDULE_RESPONSE_REJECTED :
                    MeridianGlobalConstants.XCBL_RESPONSE_TYPE_CODED_SHIPPING_SCHEDULE_RESPONSE_PENDING;

                var purposeCoded = MeridianGlobalConstants.XCBL_PURPOSE_CODED_SHIPPING_SCHEDULE_RESPONSE;

                for (int i = 0; i < 3; i++)//Loop through 3 times if got response unsuccessful
                {
                    bool isSuccess = false;
                    /*Below code to send Shipping Schedule Request to AWC*/
                    using (var client = new WebClient())
                    {
                        var data = CreateShippingScheduleResponse(meridianResult, responseTypeCoded, purposeCoded, ref shippingScheduleRequestId);
                        var requestData = Encoding.ASCII.GetBytes(data);
                        client.Headers.Add("Content-Type", "text/xml;charset=utf-8");
                        client.Headers.Add("SOAPAction", MeridianGlobalConstants.CONFIG_AWC_ACTION);
                        var responseData = client.UploadData(MeridianGlobalConstants.CONFIG_AWC_ENDPOINT, requestData);
                        var response = Encoding.ASCII.GetString(responseData);
                        isSuccess = GetCurrentShippingScheduleRequestResponse(response, data, shippingScheduleRequestId, meridianResult.OrderNumber);
                    }
                    if (isSuccess)
                        break;
                }

            }
            catch (Exception ex)
            {
               // MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "SendShippingScheduleResponse", "06.07", "Error - Send ShippingScheduleResponse Request", string.Format("Error - While sending SSR Request: {0}", ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, meridianResult.XmlDocument, "Error 06.07 - Send AWC SSR Request");
            }
        }

        internal static string CreateShippingScheduleResponse(MeridianResult meridianResult, string responseTypeCoded, string purposeCoded, ref string shipScheduleRespId)
        {
            try
            {
                var shippingScheduleResponseGUID = shipScheduleRespId = Guid.NewGuid().ToString();
                StringBuilder request = new StringBuilder();

                var shippingScheduleHeader = meridianResult.XmlDocument.GetElementsByTagName(MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_HEADER).Item(0);

                request.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                request.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns=\"rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd\" xmlns:core=\"rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd\">");
                request.Append("<soapenv:Header>");
                request.Append("<Credentials>");
                request.Append("<UserName>");
                request.Append(Encryption.Encrypt(meridianResult.WebUserName, meridianResult.WebHashKey));
                request.Append("</UserName>");
                request.Append("<Password>");
                request.Append(Encryption.Encrypt(meridianResult.WebPassword, meridianResult.WebHashKey));
                request.Append("</Password>");
                request.Append("<Hashkey>");
                request.Append(meridianResult.WebHashKey);
                request.Append("</Hashkey>");
                request.Append("</Credentials>");
                request.Append("</soapenv:Header>");
                request.Append("<soapenv:Body>");
                request.Append("<ShippingScheduleResponse>");
                request.Append("<ShippingScheduleResponseHeader>");
                request.Append("<ScheduleResponseID>");
                request.Append(shippingScheduleResponseGUID);
                request.Append("</ScheduleResponseID>");
                request.Append("<ScheduleResponseIssueDate>");
                request.Append(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                request.Append("</ScheduleResponseIssueDate>");
                request.Append("<ShippingScheduleReference>");
                request.Append("<core:RefNum>");
                request.Append(meridianResult.OrderNumber);
                request.Append("</core:RefNum>");
                request.Append("</ShippingScheduleReference>");
                request.Append("<Purpose>");
                request.Append("<core:PurposeCoded>");
                request.Append(purposeCoded);
                request.Append("</core:PurposeCoded>");
                request.Append("</Purpose>");
                request.Append("<ResponseType>");
                request.Append("<core:ResponseTypeCoded>");
                request.Append(responseTypeCoded);
                request.Append("</core:ResponseTypeCoded>");
                request.Append("</ResponseType>");
                request.Append("<ShippingScheduleHeader>");
                request.Append(shippingScheduleHeader.InnerXml);
                request.Append("</ShippingScheduleHeader>");
                if (!string.IsNullOrWhiteSpace(meridianResult.Comments))
                {
                    request.Append("<ShippingScheduleResponseHeaderNote>");
                    request.Append(meridianResult.Comments);
                    request.Append("</ShippingScheduleResponseHeaderNote>");
                }
                request.Append("</ShippingScheduleResponseHeader>");
                request.Append("</ShippingScheduleResponse>");
                request.Append("</soapenv:Body>");
                request.Append("</soapenv:Envelope>");

                var currentXmlDocument = new XmlDocument();
                currentXmlDocument.LoadXml(request.ToString());
               // MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "CreateShippingScheduleResponse", "04.01", "Success - Created ShippingScheduleResponse Request", "Shipping Schedule Response Request", "No File Name", shippingScheduleResponseGUID, meridianResult.OrderNumber, currentXmlDocument, "Success");
                return request.ToString();
            }
            catch (Exception ex)
            {
               // MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "CreateShippingScheduleResponse", "06.02", "Error - Create ShippingScheduleResponse Request", string.Format("Error - While creating SSR Request: {0}", ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, meridianResult.XmlDocument, "Error 06.02 - Create AWC SSR Request");
                return string.Empty;
            }
        }

        private static void SetPrefix(string prefix, XmlNode node)
        {
            if (!node.Prefix.Equals("core"))
                node.Prefix = prefix;
            foreach (XmlNode n in node.ChildNodes)
            {
                SetPrefix(prefix, n);
            }
        }

        internal static bool GetCurrentShippingScheduleRequestResponse(string strResponse, string strRequest, string uniqueId, string orderNumber)
        {
            var currentXmlDocument = new XmlDocument();
            currentXmlDocument.LoadXml(strRequest);

            if (!string.IsNullOrWhiteSpace(strResponse))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strResponse);
                XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                xmlNsManager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
                xmlNsManager.AddNamespace("ns0", "rrn:org.xcbl:schemas/xcbl/v4_0/messagemanagement/v1_0/messagemanagement.xsd");
                xmlNsManager.AddNamespace("es", "rrn:org.xcbl:schemas/xcbl/v4_0/externalschemas/externalschemas.xsd");
                xmlNsManager.AddNamespace("core", "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd");

                var acknowledgementNote = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_ACKNOWLEDGEMENT_NOTE).Item(0);
                if (acknowledgementNote != null)
                {
                    var currentStatus = acknowledgementNote.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_ACKNOWLEDGEMENT_SUCCESS, "05.02", new ProcessData(), uniqueId, "GetCurrentShippingScheduleRequestResponse");
                    if ((currentStatus != null) && (!string.IsNullOrWhiteSpace(currentStatus.InnerText)))
                    {
                        var isSuccess = currentStatus.InnerText.ReplaceSpecialCharsWithSpace().Trim().Equals(MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_SUCCESS, StringComparison.OrdinalIgnoreCase);
                        return isSuccess;
                    }
                }
                else
                {
                  //  MeridianSystemLibrary.LogTransaction(string.Empty, string.Empty, "GetCurrentShippingScheduleRequestResponse", "05.01", "Warning - No Acknowledgement Note", "Acknowledgement note not found", string.Empty, uniqueId, orderNumber, currentXmlDocument, "Warning");
                }
            }
            else
            {
               // MeridianSystemLibrary.LogTransaction(string.Empty, string.Empty, "GetCurrentShippingScheduleRequestResponse", "06.03", "Error - No Response", "Response not found", string.Empty, uniqueId, orderNumber, currentXmlDocument, "Error 06.03 - No AWC SSRRequest Response");
            }
            return false;
        }
           
        internal static XmlDocument UpdateShippingScheduleRequest(XmlDocument currentShippingScheduleRequest, string[] allValues)
        {

            XmlNode shippingElement = currentShippingScheduleRequest.GetElementsByTagName(MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_HEADER).Item(0);
            XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(currentShippingScheduleRequest.NameTable);
            xmlNsManager.AddNamespace("default", "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd");
            xmlNsManager.AddNamespace("core", "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd");

            /*To update the Other Schedule References*/
           // UpdateOtherScheduleReferences(xmlNsManager, shippingElement, allValues);

            return currentShippingScheduleRequest;
        }

        /// <summary>
        /// To get Other schedule references
        /// </summary>
        /// <param name="xmlNsManager"> XmlNamespaceManager </param>
        /// <param name="xnScheduleReferences">ScheduleReferences xml node from requested data </param>
        /// <param name="allValues">values from PBS FTP file</param>
        //private static void UpdateOtherScheduleReferences(XmlNamespaceManager xmlNsManager, XmlNode xnShippingElement, string[] allValues)
        //{
        //    XmlNode xnOtherScheduleReferences = xnShippingElement.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_OTHER_SCHEDULE_REFERENCES, "05", new ProcessData(), allValues[0], "UpdateOtherScheduleReferences");
        //    if (xnOtherScheduleReferences != null)
        //    {
        //        XmlNodeList xnReferenceCoded = xnOtherScheduleReferences.ChildNodes; // 8Nodes
        //        for (int iReferenceCodedIndex = 0; iReferenceCodedIndex < xnReferenceCoded.Count; iReferenceCodedIndex++)
        //        {
        //            XmlNodeList xnReferences = xnReferenceCoded[iReferenceCodedIndex].ChildNodes;
        //            if (xnReferences.Count == 3
        //                && ((xnReferences[1].Name.Trim().Equals(string.Format("core:{0}", MeridianGlobalConstants.XCBL_REFERENCE_TYPECODE_OTHER), StringComparison.OrdinalIgnoreCase)
        //                && xnReferences[2].Name.Trim().Equals(string.Format("core:{0}", MeridianGlobalConstants.XCBL_REFERENCE_DESCRIPTION), StringComparison.OrdinalIgnoreCase)) ||
        //                 (xnReferences[1].Name.Trim().Equals(MeridianGlobalConstants.XCBL_REFERENCE_TYPECODE_OTHER, StringComparison.OrdinalIgnoreCase)
        //                && xnReferences[2].Name.Trim().Equals(MeridianGlobalConstants.XCBL_REFERENCE_DESCRIPTION, StringComparison.OrdinalIgnoreCase))))
        //            {
        //                var currentItem = xnReferences[1].InnerText;
        //                if (!string.IsNullOrWhiteSpace(xnReferences[1].InnerText))
        //                {
        //                    switch (xnReferences[1].InnerText.Trim().Replace(" ", "").ToLower())
        //                    {
        //                        case MeridianGlobalConstants.XCBL_FIRST_STOP:
        //                            xnReferences[2].InnerText = allValues[5];
        //                            break;
        //                        case MeridianGlobalConstants.XCBL_BEFORE_7:
        //                            xnReferences[2].InnerText = allValues[6];
        //                            break;
        //                        case MeridianGlobalConstants.XCBL_BEFORE_9:
        //                            xnReferences[2].InnerText = allValues[7];
        //                            break;
        //                        case MeridianGlobalConstants.XCBL_BEFORE_12:
        //                            xnReferences[2].InnerText = allValues[8];
        //                            break;
        //                        case MeridianGlobalConstants.XCBL_SAME_DAY:
        //                            xnReferences[2].InnerText = allValues[9];
        //                            break;
        //                        case MeridianGlobalConstants.XCBL_HOME_OWNER_OCCUPIED:
        //                            xnReferences[2].InnerText = allValues[10];
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// To validate the request and call correct class method
        /// </summary>
        /// <param name="operationContext"> CurrentOperationContext </param>
        internal static bool IsShippingScheduleRequest(OperationContext operationContext)
        {
            try
            {
                var requestMessage = operationContext.RequestContext.RequestMessage.ToString().ReplaceSpecialCharsWithSpace();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(requestMessage);

                XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                xmlNsManager.AddNamespace("default", "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd");
                xmlNsManager.AddNamespace("core", "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd");

                XmlNodeList shippingElement = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_HEADER);
                return (shippingElement != null && (shippingElement.Count > 0));
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex);
                return true;
            }
        }
    }
}