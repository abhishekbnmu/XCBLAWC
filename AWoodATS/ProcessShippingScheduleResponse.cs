using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;

namespace AWoodATS
{
    public class ProcessShippingScheduleResponse
    {
        private MeridianResult _meridianResult = null;

        #region Shipping Schedule Response Request

        /// <summary>
        /// Method to pass xCBL XML data to the web serivce
        /// </summary>
        /// <param name="currentOperationContext">Operation context inside this XmlElement the xCBL XML data to parse</param>
        /// <returns>XElement - XML Message Acknowledgement response indicating Success or Failure</returns>
        internal MeridianResult ProcessShippingScheduleResponseRequest(OperationContext currentOperationContext)
        {
            _meridianResult = new MeridianResult();
            _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_SUCCESS;

            XCBL_User xCblServiceUser = new XCBL_User();
            //MeridianSystemLibrary.LogTransaction("No WebUser", "No FTPUser", "ProcessShippingScheduleResponseDocument", "01.01", "Success - New SOAP Request Received", "Shipping Schedule Response Process", "No FileName", "No Schedule ID", "No Order Number", null, "Success");
            if (CommonProcess.IsAuthenticatedRequest(currentOperationContext, ref xCblServiceUser))
            {
                //MeridianSystemLibrary.LogTransaction(xCblServiceUser.WebUsername, xCblServiceUser.FtpUsername, "IsAuthenticatedRequest", "01.02", "Success - Authenticated request", "Shipping Schedule Response Process", "No FileName", "No Schedule ID", "No Order Number", null, "Success");
                ProcessData processData = ProcessRequestAndCreateFiles(currentOperationContext, xCblServiceUser);
                if (processData == null || string.IsNullOrEmpty(processData.ScheduleResponseID) || string.IsNullOrEmpty(processData.OrderNumber))
                    _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;
                else
                {
                    processData.FtpUserName = xCblServiceUser.FtpUsername;
                    processData.FtpPassword = xCblServiceUser.FtpPassword;
                    processData.FtpServerInFolderPath = xCblServiceUser.FtpServerInFolderPath;
                    processData.FtpServerOutFolderPath = xCblServiceUser.FtpServerOutFolderPath;
                    processData.LocalFilePath = xCblServiceUser.LocalFilePath;
                    _meridianResult.WebUserName = xCblServiceUser.WebUsername;
                    _meridianResult.WebPassword = xCblServiceUser.WebPassword;
                    _meridianResult.WebHashKey = xCblServiceUser.Hashkey;

                    if (!CreateLocalCsvFile(processData))
                        _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;
                    _meridianResult.UniqueID = processData.ScheduleResponseID;
                    return _meridianResult;
                }
            }
            else
            {
                _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;
                //MeridianSystemLibrary.LogTransaction("No WebUser", "No FTPUser", "IsAuthenticatedRequest", "03.01", "Error - New SOAP Request not authenticated", "UnAuthenticated Request", "No FileName", "No Schedule ID", "No Order Number", null, "Error 03.01 - Incorrect Credential");
            }
            return _meridianResult;
        }

        /// <summary>
        /// To Process request and create csv and xml files.
        /// </summary>
        /// <param name="operationContext">Current OperationContext</param>
        /// <returns></returns>
        private ProcessData ProcessRequestAndCreateFiles(OperationContext operationContext, XCBL_User xCblServiceUser)
        {
            try
            {
                ProcessData processData = ValidateScheduleShippingResponseXmlDocument(operationContext.RequestContext, xCblServiceUser);
                if (processData != null && !string.IsNullOrEmpty(processData.ScheduleResponseID)
                    && !string.IsNullOrEmpty(processData.OrderNumber)
                   && !string.IsNullOrEmpty(processData.CsvFileName))

                {
                    //MeridianSystemLibrary.LogTransaction(xCblServiceUser.WebUsername, xCblServiceUser.FtpUsername, "ProcessRequestAndCreateFiles", "01.03", string.Format("Success - Parsed requested xml for CSV file {0}", processData.ScheduleResponseID), "Shipping Schedule Response Process", processData.CsvFileName, processData.ScheduleResponseID, processData.OrderNumber, processData.XmlDocument, "Success");
                    return processData;
                }
            }
            catch (Exception ex)
            {
               // MeridianSystemLibrary.LogTransaction(xCblServiceUser.WebUsername, xCblServiceUser.FtpUsername, "ValidateScheduleShippingXmlDocument", "03.02", "Error - Incorrect request ", string.Format("Exception - Invalid request xml {0}", ex.Message), "No file Name", "No Schedule Id", "No Order Number", null, "Error 03.02 - Invalid request xml");
            }

            return new ProcessData();
        }

        /// <summary>
        /// To create CSV file
        /// </summary>
        /// <param name="processData">Process data</param>
        /// <returns></returns>
        private bool CreateLocalCsvFile(ProcessData processData)
        {
            bool result = false;
            try
            {
                if (processData != null && !string.IsNullOrEmpty(processData.ScheduleResponseID)
                     && !string.IsNullOrEmpty(processData.OrderNumber)
                    && !string.IsNullOrEmpty(processData.CsvFileName))
                {
                    var record = string.Format(MeridianGlobalConstants.SHIPPING_SCHEDULE_RESPONSE_CSV_HEADER_NAMES_FORMAT,
                       processData.ShippingScheduleResponse.ScheduleResponseID, processData.ShippingScheduleResponse.ScheduleResponseIssueDate, processData.ShippingScheduleResponse.ScheduleResponseOrderNumber, processData.ShippingScheduleResponse.ScheduleResponsePurposeCoded, processData.ShippingScheduleResponse.ScheduleResponseTypeCoded, "",
                       processData.ShippingScheduleResponse.ScheduleID, processData.ShippingScheduleResponse.ScheduleIssuedDate, processData.ShippingScheduleResponse.OrderNumber, processData.ShippingScheduleResponse.SequenceNumber,
                       processData.ShippingScheduleResponse.Other_FirstStop, processData.ShippingScheduleResponse.Other_Before7, processData.ShippingScheduleResponse.Other_Before9, processData.ShippingScheduleResponse.Other_Before12, processData.ShippingScheduleResponse.Other_SameDay, processData.ShippingScheduleResponse.Other_OwnerOccupied, processData.ShippingScheduleResponse.Other_7, processData.ShippingScheduleResponse.Other_8, processData.ShippingScheduleResponse.Other_9, processData.ShippingScheduleResponse.Other_10,
                       processData.ShippingScheduleResponse.PurposeCoded, processData.ShippingScheduleResponse.ScheduleType, processData.ShippingScheduleResponse.AgencyCoded, processData.ShippingScheduleResponse.Name1, processData.ShippingScheduleResponse.Street, processData.ShippingScheduleResponse.StreetSupplement1, processData.ShippingScheduleResponse.PostalCode, processData.ShippingScheduleResponse.City, processData.ShippingScheduleResponse.RegionCoded,
                       processData.ShippingScheduleResponse.ContactName, processData.ShippingScheduleResponse.ContactNumber_1, processData.ShippingScheduleResponse.ContactNumber_2, processData.ShippingScheduleResponse.ContactNumber_3, processData.ShippingScheduleResponse.ContactNumber_4, processData.ShippingScheduleResponse.ContactNumber_5, processData.ShippingScheduleResponse.ContactNumber_6,
                       processData.ShippingScheduleResponse.ShippingInstruction, processData.ShippingScheduleResponse.GPSSystem, processData.ShippingScheduleResponse.Latitude.ToString(), processData.ShippingScheduleResponse.Longitude.ToString(), processData.ShippingScheduleResponse.LocationID, processData.ShippingScheduleResponse.EstimatedArrivalDate, processData.ShippingScheduleResponse.OrderType);
                    StringBuilder strBuilder = new StringBuilder(MeridianGlobalConstants.SHIPPING_SCHEDULE_RESPONSE_CSV_HEADER_NAMES);
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(record);
                    string csvContent = strBuilder.ToString();

                    _meridianResult.FtpUserName = processData.FtpUserName;
                    _meridianResult.FtpPassword = processData.FtpPassword;
                    _meridianResult.FtpServerInFolderPath = processData.FtpServerInFolderPath;
                    _meridianResult.FtpServerOutFolderPath = processData.FtpServerOutFolderPath;
                    _meridianResult.LocalFilePath = processData.LocalFilePath;
                    _meridianResult.WebUserName = processData.WebUserName;
                    _meridianResult.UniqueID = processData.ScheduleResponseID;
                    _meridianResult.OrderNumber = processData.OrderNumber;
                    _meridianResult.FileName = processData.CsvFileName;

                    if (MeridianGlobalConstants.CONFIG_CREATE_LOCAL_CSV == MeridianGlobalConstants.SHOULD_CREATE_LOCAL_FILE)
                    {
                        _meridianResult.UploadFromLocalPath = true;
                        return CommonProcess.CreateFile(csvContent, _meridianResult);
                    }
                    else
                    {
                        byte[] content = Encoding.UTF8.GetBytes(csvContent);
                        int length = content.Length;

                        if (!string.IsNullOrEmpty(processData.CsvFileName) && length > 40)
                        {
                            _meridianResult.Content = content;
                            result = true;
                        }
                        else
                        {
                           // MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "CreateLocalCsvFile", "03.06", ("Error - Creating CSV File because of Stream " + length), string.Format("Error - Creating CSV File {0} with error of Stream", processData.CsvFileName), processData.CsvFileName, processData.ScheduleResponseID, processData.OrderNumber, processData.XmlDocument, "Error 03.06 - Create CSV File");
                        }
                    }
                }
                else
                {
                   // MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "CreateLocalCsvFile", "03.06", "Error - Creating CSV File because of Process DATA", string.Format("Error - Creating CSV File {0} with error of Process DATA", processData.CsvFileName), processData.CsvFileName, processData.ScheduleResponseID, processData.OrderNumber, processData.XmlDocument, "Error 03.06 - Create CSV File");
                }
            }
            catch (Exception ex)
            {
               // MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "CreateLocalCsvFile", "03.06", "Error - Creating CSV File", string.Format("Error - Creating CSV File {0} with error {1}", processData.CsvFileName, ex.Message), processData.CsvFileName, processData.ScheduleResponseID, processData.OrderNumber, processData.XmlDocument, "Error 03.06 - Create CSV File");
            }

            return result;
        }

        #region XML Parsing

        /// <summary>
        /// To Parse sent SOAP XML and make list of Process data
        /// </summary>
        /// <param name="requestContext"> Current OperationContext's RequestContext</param>
        /// <param name="xCblServiceUser">Service User</param>
        /// <returns>List of process data</returns>
        private ProcessData ValidateScheduleShippingResponseXmlDocument(RequestContext requestContext, XCBL_User xCblServiceUser)
        {
            var requestMessage = requestContext.RequestMessage.ToString().ReplaceSpecialCharsWithSpace();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(requestMessage);

            XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNsManager.AddNamespace("default", "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd");
            xmlNsManager.AddNamespace("core", "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd");

            XmlNodeList shippingElement = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_RESPONSE_HEADER);



            //Find the Shipping schedule response tag and getting the Inner Xml of its Node
            XmlNodeList shippingScheduleResponseNode_xml = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_ShippingScheuleResponse_XML_Http);//Http Request creating this tag
            if (shippingScheduleResponseNode_xml.Count == 0)
            {
                shippingScheduleResponseNode_xml = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_ShippingScheuleResponse_XML_Https);//Https Request creating this tag
            }

            if (shippingElement != null)
            {
                // There should only be one element in the Shipping Schedule request, but this should handle multiple ones
                foreach (XmlNode element in shippingElement)
                {
                    var processData = xCblServiceUser.GetNewProcessData();
                    processData.XmlDocument = xmlDoc;
                    _meridianResult.XmlDocument = xmlDoc;

                    var scheduleResponseId = element.GetNodeByNameAndLogErrorTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_RESPONSE_ID, "10", processData, processData.ScheduleResponseID);
                    var scheduleResponseIssuedDate = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_RESPONSE_ISSUE_DATE, "21", processData, processData.ScheduleResponseID);
                    var scheduleResponseReference = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_RESPONSE_REFERENCE, "22", processData, processData.ScheduleResponseID);
                    var scheduleResponsePurposeCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_RESPONSE_PURPOSE_CODED, "03", processData, processData.ScheduleResponseID);
                    var scheduleResponseResponseTypeCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_RESPONSE_RESPONSE_TYPE_CODED, "23", processData, processData.ScheduleResponseID);

                    if (scheduleResponseId != null && !string.IsNullOrEmpty(scheduleResponseId.InnerText))
                    {
                        processData.ScheduleResponseID = scheduleResponseId.InnerText.ReplaceSpecialCharsWithSpace();
                        processData.ShippingScheduleResponse.ScheduleResponseID = processData.ScheduleResponseID;

                        if (scheduleResponseIssuedDate != null && !string.IsNullOrEmpty(scheduleResponseIssuedDate.InnerText))
                            processData.ShippingScheduleResponse.ScheduleResponseIssueDate = scheduleResponseIssuedDate.InnerText.ReplaceSpecialCharsWithSpace();

                        if (scheduleResponseReference != null && !string.IsNullOrEmpty(scheduleResponseReference.InnerText))
                        {
                            processData.OrderNumber = scheduleResponseReference.InnerText.ReplaceSpecialCharsWithSpace();
                            processData.ShippingScheduleResponse.ScheduleResponseOrderNumber = processData.OrderNumber;

                            string formattedOrderNumber = processData.OrderNumber.ReplaceSpecialCharsWithSpace().Replace(" ", "");
                            string fileNameFormat = DateTime.Now.ToString(MeridianGlobalConstants.XCBL_FILE_DATETIME_FORMAT);
                            processData.CsvFileName = string.Concat(MeridianGlobalConstants.XCBL_AWC_FILE_PREFIX, fileNameFormat, formattedOrderNumber, MeridianGlobalConstants.XCBL_FILE_EXTENSION);
                            processData.XmlFileName = string.Concat(MeridianGlobalConstants.XCBL_AWC_FILE_PREFIX, fileNameFormat, formattedOrderNumber, MeridianGlobalConstants.XCBL_XML_EXTENSION);
                        }
                        //else
                        //    MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "ValidateScheduleShippingResponseXmlDocument", "03.11", "Error - Schedule Response References XML tag missing or incorrect to get order number", "Exception - Response order number", processData.CsvFileName, processData.ScheduleResponseID, "No Order Number", processData.XmlDocument, "Error 03.14 - Seller OrderNumber not found");


                        if (scheduleResponsePurposeCoded != null && !string.IsNullOrEmpty(scheduleResponsePurposeCoded.InnerText))
                            processData.ShippingScheduleResponse.ScheduleResponsePurposeCoded = scheduleResponsePurposeCoded.InnerText.ReplaceSpecialCharsWithSpace();

                        if (scheduleResponseResponseTypeCoded != null && !string.IsNullOrEmpty(scheduleResponseResponseTypeCoded.InnerText))
                            processData.ShippingScheduleResponse.ScheduleResponseTypeCoded = scheduleResponseResponseTypeCoded.InnerText.ReplaceSpecialCharsWithSpace();

                        if (string.IsNullOrWhiteSpace(processData.ShippingScheduleResponse.ScheduleResponseID) || string.IsNullOrWhiteSpace(processData.ShippingScheduleResponse.ScheduleResponseOrderNumber))
                            break;

                        var scheduleId = element.GetNodeByNameAndLogErrorTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_ID, "03", processData, processData.ScheduleResponseID);
                        var scheduleIssuedDate = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_ISSUED_DATE, "01", processData, processData.ScheduleResponseID);

                        //Schedule Header Information --start
                        if (scheduleId != null && !string.IsNullOrEmpty(scheduleId.InnerText))
                        {
                            processData.ShippingScheduleResponse.ScheduleID = scheduleId.InnerText.ReplaceSpecialCharsWithSpace();

                            if (scheduleIssuedDate != null && !string.IsNullOrEmpty(scheduleIssuedDate.InnerText))
                                processData.ShippingScheduleResponse.ScheduleIssuedDate = scheduleIssuedDate.InnerText.ReplaceSpecialCharsWithSpace();


                            XmlNode xnScheduleReferences = element.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_REFERENCES, "02", processData, processData.ScheduleResponseID);

                            if (xnScheduleReferences != null)
                                GetPurchaseOrderReference(xmlNsManager, xnScheduleReferences, processData);
                            //else if (string.IsNullOrEmpty(processData.ShippingScheduleResponse.OrderNumber))
                            //    MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "ValidateScheduleShippingResponseXmlDocument", "03.04", "Error - Schedule References XML tag missing or incorrect to get seller order number", "Exception - Seller order number", processData.CsvFileName, processData.ScheduleResponseID, "No Order Number", processData.XmlDocument, "Error 03.04 - Seller OrderNumber not found");

                            if (string.IsNullOrEmpty(processData.ShippingScheduleResponse.ScheduleID) || string.IsNullOrEmpty(processData.ShippingScheduleResponse.OrderNumber))
                                break;

                            else
                            {
                                GetOtherScheduleReferences(xmlNsManager, xnScheduleReferences, processData);

                                GetPurposeScheduleTypeCodeAndParty(xmlNsManager, element, processData);

                                GetListOfContactNumber(xmlNsManager, element, processData);

                                GetListOfTransportRouting(xmlNsManager, element, processData);

                                return processData;
                            }
                        }
                    }
                }
            }
            //else
            //    MeridianSystemLibrary.LogTransaction(xCblServiceUser.WebUsername, xCblServiceUser.FtpUsername, "ValidateScheduleShippingResponseXmlDocument", "03.02", "Error - Shipping Schedule Response Header XML tag missing or incorrect", "Exception - Invalid request xml", "No file Name", "No Schedule Id", "No Order Number", xmlDoc, "Error 03.02 - Invalid request xml");
            return new ProcessData();
        }

        /// <summary>
        /// To get seller order number and sequence
        /// </summary>
        /// <param name="xmlNsManager"> XmlNamespaceManager </param>
        /// <param name="xnScheduleReferences">ScheduleReferences xml node from requested data </param>
        /// <param name="processData">Process data</param>
        private void GetPurchaseOrderReference(XmlNamespaceManager xmlNsManager, XmlNode xnScheduleReferences, ProcessData processData)
        {
            XmlNode xnPurchaseOrderReferences = xnScheduleReferences.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_PURCHASE_ORDER_REFERENCE, "04", processData, processData.ScheduleResponseID, "GetPurchaseOrderReference");
            if (xnPurchaseOrderReferences != null)
            {
                XmlNodeList xnlPurchaseOrderReferences = xnPurchaseOrderReferences.ChildNodes;
                for (int iPurchaseOrderIndex = 0; iPurchaseOrderIndex < xnlPurchaseOrderReferences.Count; iPurchaseOrderIndex++)
                {
                    if (xnlPurchaseOrderReferences[iPurchaseOrderIndex].Name.Contains(MeridianGlobalConstants.XCBL_SELLER_ORDER_NUMBER))
                        processData.ShippingScheduleResponse.OrderNumber = xnlPurchaseOrderReferences[iPurchaseOrderIndex].InnerText.ReplaceSpecialCharsWithSpace();

                    if (xnlPurchaseOrderReferences[iPurchaseOrderIndex].Name.Contains(MeridianGlobalConstants.XCBL_ORDER_TYPE))
                    {
                        XmlNodeList xnlOrderType = xnlPurchaseOrderReferences[iPurchaseOrderIndex].ChildNodes;
                        for (int iOrderType = 0; iOrderType < xnlOrderType.Count; iOrderType++)
                        {
                            if (xnlOrderType[iOrderType].Name.Contains(MeridianGlobalConstants.XCBL_ORDER_TYPE_CODED_OTHER))
                                processData.ShippingScheduleResponse.OrderType = xnlOrderType[iOrderType].InnerText.ReplaceSpecialCharsWithSpace();
                        }
                    }

                    if (xnlPurchaseOrderReferences[iPurchaseOrderIndex].Name.Contains(MeridianGlobalConstants.XCBL_CHANGE_ORDER_SEQUENCE_NUMBER))
                        processData.ShippingScheduleResponse.SequenceNumber = xnlPurchaseOrderReferences[iPurchaseOrderIndex].InnerText.ReplaceSpecialCharsWithSpace();
                }
            }
        }

        /// <summary>
        /// To get Other schedule references
        /// </summary>
        /// <param name="xmlNsManager"> XmlNamespaceManager </param>
        /// <param name="xnScheduleReferences">ScheduleReferences xml node from requested data </param>
        /// <param name="processData">Process data</param>
        private void GetOtherScheduleReferences(XmlNamespaceManager xmlNsManager, XmlNode xnScheduleReferences, ProcessData processData)
        {
            XmlNode xnOtherScheduleReferences = xnScheduleReferences.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_OTHER_SCHEDULE_REFERENCES, "05", processData, processData.ScheduleResponseID, "GetOtherScheduleReferences");
            if (xnOtherScheduleReferences != null)
            {
                XmlNodeList xnReferenceCoded = xnOtherScheduleReferences.ChildNodes; // 8Nodes
                for (int iReferenceCodedIndex = 0; iReferenceCodedIndex < xnReferenceCoded.Count; iReferenceCodedIndex++)
                {
                    XmlNodeList xnReferences = xnReferenceCoded[iReferenceCodedIndex].ChildNodes;
                    if (xnReferences.Count == 3
                        && ((xnReferences[1].Name.Trim().Equals(string.Format("core:{0}", MeridianGlobalConstants.XCBL_REFERENCE_TYPECODE_OTHER), StringComparison.OrdinalIgnoreCase)
                        && xnReferences[2].Name.Trim().Equals(string.Format("core:{0}", MeridianGlobalConstants.XCBL_REFERENCE_DESCRIPTION), StringComparison.OrdinalIgnoreCase)) ||
                         (xnReferences[1].Name.Trim().Equals(MeridianGlobalConstants.XCBL_REFERENCE_TYPECODE_OTHER, StringComparison.OrdinalIgnoreCase)
                        && xnReferences[2].Name.Trim().Equals(MeridianGlobalConstants.XCBL_REFERENCE_DESCRIPTION, StringComparison.OrdinalIgnoreCase))))
                        processData.ShippingScheduleResponse.SetOtherScheduleReferenceDesc(xnReferences[1].InnerText, xnReferences[2].InnerText.ReplaceSpecialCharsWithSpace());
                }
            }
        }

        /// <summary>
        /// To get single node value
        /// </summary>
        /// <param name="xmlNsManager"> XmlNamespaceManager </param>
        /// <param name="element">Main xml node from requested data </param>
        /// <param name="processData">Process data</param>
        private void GetPurposeScheduleTypeCodeAndParty(XmlNamespaceManager xmlNsManager, XmlNode element, ProcessData processData)
        {
            XmlNode purposeCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_PURPOSE_CODED, "03", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (purposeCoded != null)
                processData.ShippingScheduleResponse.PurposeCoded = purposeCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode scheduleTypeCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_TYPE_CODED, "04", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (scheduleTypeCoded != null)
                processData.ShippingScheduleResponse.ScheduleType = scheduleTypeCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode agencyCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_AGENCY_CODED, "05", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (agencyCoded != null)
                processData.ShippingScheduleResponse.AgencyCoded = agencyCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode name1 = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_NAME, "06", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (name1 != null)
                processData.ShippingScheduleResponse.Name1 = name1.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode street = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_STREET, "07", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (street != null)
                processData.ShippingScheduleResponse.Street = street.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode streetSupplement1 = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_STREET_SUPPLEMENT, "08", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (streetSupplement1 != null)
                processData.ShippingScheduleResponse.StreetSupplement1 = streetSupplement1.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode postalCode = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_POSTAL_CODE, "09", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (postalCode != null)
                processData.ShippingScheduleResponse.PostalCode = postalCode.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode city = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_CITY, "10", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (city != null)
                processData.ShippingScheduleResponse.City = city.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode regionCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_REGION_CODED, "11", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (regionCoded != null)
                processData.ShippingScheduleResponse.RegionCoded = regionCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode contactName = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_CONTACT_NAME, "12", processData, processData.ScheduleResponseID, MeridianGlobalConstants.METHOD_GET_PURPOSE_SCHEDULE_TYPE_CODE_AND_PARTY);
            if (contactName != null)
                processData.ShippingScheduleResponse.ContactName = contactName.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();
        }

        /// <summary>
        /// To get List of contacts
        /// </summary>
        /// <param name="xmlNsManager"> XmlNamespaceManager </param>
        /// <param name="element">Main xml node from requested data </param>
        /// <param name="processData">Process data</param>
        private void GetListOfContactNumber(XmlNamespaceManager xmlNsManager, XmlNode element, ProcessData processData)
        {
            var lisOfContactNumber = element.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LIST_OF_CONTACT_NUMBERS, "13", processData, processData.ScheduleResponseID, "GetListOfContactNumber");
            if (lisOfContactNumber != null && lisOfContactNumber.ChildNodes != null)
            {
                XmlNodeList xnlContactNames = lisOfContactNumber.ChildNodes;
                for (int iContactNameIndex = 0; iContactNameIndex < xnlContactNames.Count; iContactNameIndex++)
                {
                    XmlNodeList xnlContactValues = xnlContactNames[iContactNameIndex].ChildNodes;
                    for (int iContactValuesIndex = 0; iContactValuesIndex < xnlContactValues.Count; iContactValuesIndex++)
                        if (xnlContactValues[iContactValuesIndex].Name.Contains(MeridianGlobalConstants.XCBL_CONTACT_VALUE))
                            processData.ShippingScheduleResponse.SetContactNumbers(xnlContactValues[iContactValuesIndex].InnerText.ReplaceSpecialCharsWithSpace(), iContactNameIndex);
                }
            }
            //else if (lisOfContactNumber.ChildNodes == null)
            //    MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "GetListOfContactNumber", "14", "Warning - The Contact Number Not Found.", "Warning - Contact Number", processData.CsvFileName, processData.ScheduleResponseID, processData.OrderNumber, null, "Warning 14 - Contact Name Not Found");

        }

        /// <summary>
        /// To get transportion data
        /// </summary>
        /// <param name="xmlNsManager"> XmlNamespaceManager </param>
        /// <param name="element">Main xml node from requested data </param>
        /// <param name="processData">Process data</param>
        private void GetListOfTransportRouting(XmlNamespaceManager xmlNsManager, XmlNode element, ProcessData processData)
        {

            XmlNode shippingInstruction = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SHIPPING_INSTRUCTIONS, "15", processData, processData.ScheduleResponseID);
            if (shippingInstruction != null)
                processData.ShippingScheduleResponse.ShippingInstruction = shippingInstruction.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode gpsSystem = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_GPS_SYSTEM, "16", processData, processData.ScheduleResponseID);
            if (gpsSystem != null)
                processData.ShippingScheduleResponse.GPSSystem = gpsSystem.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode latitude = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LATITUDE, "17", processData, processData.ScheduleResponseID);
            if (latitude != null)
            {
                double dLatitude;
                double.TryParse(latitude.InnerText.ReplaceSpecialCharsWithSpace(), out dLatitude);
                processData.ShippingScheduleResponse.Latitude = dLatitude;

            }

            XmlNode longitude = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LONGITUDE, "18", processData, processData.ScheduleResponseID);
            if (longitude != null)
            {
                double dLongitude;
                double.TryParse(longitude.InnerText.ReplaceSpecialCharsWithSpace(), out dLongitude);
                processData.ShippingScheduleResponse.Longitude = dLongitude;
            }

            XmlNode locationID = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LOCATION_ID, "19", processData, processData.ScheduleResponseID);
            if (locationID != null)
                processData.ShippingScheduleResponse.LocationID = locationID.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode estimatedArrivalDate = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_ESTIMATED_ARRIVAL_DATE, "20", processData, processData.ScheduleResponseID);
            if (estimatedArrivalDate != null)
                processData.ShippingScheduleResponse.EstimatedArrivalDate = estimatedArrivalDate.InnerText.ReplaceSpecialCharsWithSpace();

        }


        #endregion

        #endregion Shipping Schedule Response Request
    }
}