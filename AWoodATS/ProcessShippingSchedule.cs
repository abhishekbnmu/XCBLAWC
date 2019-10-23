using System;
using System.Data;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AWoodATS
{
    public class ProcessShippingSchedule
    {
        private MeridianResult _meridianResult = null;

        internal MeridianResult ProcessDocument(OperationContext currentOperationContext)
        {
            _meridianResult = new MeridianResult();
            _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_SUCCESS;

              XCBL_User xCblServiceUser = new XCBL_User();

            ProcessData processDatatest = ProcessRequestAndCreateFiles(currentOperationContext, xCblServiceUser);
            if (processDatatest != null)
                  WriteScheduleShippingXmlDocument(currentOperationContext.RequestContext);


            if (CommonProcess.IsAuthenticatedRequest(currentOperationContext, ref xCblServiceUser))
            {
                ApplicationError.WriteMessage("AuthenticatedRequest Sucess - New SOAP Request authenticated.");
                ProcessData processData = ProcessRequestAndCreateFiles(currentOperationContext, xCblServiceUser);
                if (processData == null || string.IsNullOrEmpty(processData.ScheduleID) || string.IsNullOrEmpty(processData.OrderNumber))
                    _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;
                else
                {     
                    if (!UpdateToServer(processData))
                    {
                       // ApplicationError.WriteMessage("NOT FOUND Order Ref Number For Process Is " + processData.ShippingSchedule.OrderNumber);
                        _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;
                    }                       
                    _meridianResult.UniqueID = processData.ScheduleID;
                    return _meridianResult;
                }
            }
            else
            {
                _meridianResult.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;                
                ApplicationError.WriteMessage("Error - New SOAP Request not authenticated. UnAuthenticated Request");
            }
            return _meridianResult;
        }

        private ProcessData ProcessRequestAndCreateFiles(OperationContext operationContext, XCBL_User xCblServiceUser)
        {
            try
            {
                ProcessData processData = ValidateScheduleShippingXmlDocument(operationContext.RequestContext, xCblServiceUser);
                if (processData != null && !string.IsNullOrEmpty(processData.ScheduleID) && !string.IsNullOrEmpty(processData.OrderNumber))
                 {
                   
                    if (UsePBSServiceDataAndUpdateFlags(processData))
                        return processData;
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex);
                ApplicationError.logErrors(ex, "ProcessRequestAndCreateFiles");
            }

            return new ProcessData();
        }

        
        private ProcessData ValidateScheduleShippingXmlDocument(RequestContext requestContext, XCBL_User xCblServiceUser)
        {
            ApplicationError.WriteMessage("ValidateScheduleShippingXmlDocument Starts");
            var requestMessage = requestContext.RequestMessage.ToString().ReplaceSpecialCharsWithSpace();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(requestMessage);

            try
            {
             string path=   Path.Combine(System.Configuration.ConfigurationManager.AppSettings["XmlPath"].ToString()+"\\" + DateTime.Now.ToString("yyyyMMdd HHss") + ".XML");
                using (XmlTextWriter writer = new XmlTextWriter(path, null))
                {
                    writer.Formatting = Formatting.Indented;
                    xmlDoc.Save(writer);
                }
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex);
                ApplicationError.logErrors(ex, "ValidateScheduleShippingXmlDocument");
            }         

            XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNsManager.AddNamespace("default", "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd");
            xmlNsManager.AddNamespace("core", "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd");
            XmlNodeList shippingElement = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_HEADER);


            //Find the Shipping schedule tag and getting the Inner Xml of its Node
            XmlNodeList shippingScheduleNode_xml = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_ShippingScheule_XML_Http);//Http Request creating this tag
            if (shippingScheduleNode_xml.Count == 0)
            {
                shippingScheduleNode_xml = xmlDoc.GetElementsByTagName(MeridianGlobalConstants.XCBL_ShippingScheule_XML_Https);//Https Request creating this tag
            }
            
            if (shippingElement != null)
            {
                // There should only be one element in the Shipping Schedule request, but this should handle multiple ones
                ApplicationError.WriteMessage("Shipping Schedule Header XML tag are correct processing data");
                foreach (XmlNode element in shippingElement)
                {
                    var processData = xCblServiceUser.GetNewProcessData();
                    processData.XmlDocument = xmlDoc;
                    _meridianResult.XmlDocument = xmlDoc;

                    var scheduleId = element.GetNodeByNameAndLogErrorTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_ID, "03", processData, processData.ScheduleID);
                    var scheduleIssuedDate = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_ISSUED_DATE, "01", processData, processData.ScheduleID);

                    //Schedule Header Information --start
                    if (scheduleId != null && !string.IsNullOrEmpty(scheduleId.InnerText))
                    {
                        processData.ScheduleID = scheduleId.InnerText.ReplaceSpecialCharsWithSpace();
                        processData.ShippingSchedule.ScheduleID = processData.ScheduleID;

                        if (scheduleIssuedDate != null && !string.IsNullOrEmpty(scheduleIssuedDate.InnerText))
                            processData.ShippingSchedule.ScheduleIssuedDate = scheduleIssuedDate.InnerText.ReplaceSpecialCharsWithSpace();


                        XmlNode xnScheduleReferences = element.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_REFERENCES, "02", processData, processData.ScheduleID);

                        if (xnScheduleReferences != null)
                            GetPurchaseOrderReference(xmlNsManager, xnScheduleReferences, processData);
                        else if (string.IsNullOrEmpty(processData.ShippingSchedule.OrderNumber))
                             ApplicationError.WriteMessage("Error - Schedule References XML tag missing or incorrect to get seller order number No Order Number found");
                        if (string.IsNullOrEmpty(processData.ShippingSchedule.ScheduleID) || string.IsNullOrEmpty(processData.ShippingSchedule.OrderNumber))
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
            else
                ApplicationError.WriteMessage("Shipping Schedule Header XML tag missing or incorrect Invalid request xml");            
            return new ProcessData();
        }


        private ProcessData WriteScheduleShippingXmlDocument(RequestContext requestContext)
        {
            ApplicationError.WriteMessage("WriteScheduleShippingXmlDocument Starts");
            var requestMessage = requestContext.RequestMessage.ToString().ReplaceSpecialCharsWithSpace();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(requestMessage);

            try
            {
                string path = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["FTPXMLPath"].ToString() + "\\" + DateTime.Now.ToString("yyyyMMdd HHss") + ".XML");
                using (XmlTextWriter writer = new XmlTextWriter(path, null))
                {
                    writer.Formatting = Formatting.Indented;
                    xmlDoc.Save(writer);
                }
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex);
                ApplicationError.logErrors(ex, "WriteScheduleShippingXmlDocument");
            }             
            return new ProcessData();
        }
        private void GetPurchaseOrderReference(XmlNamespaceManager xmlNsManager, XmlNode xnScheduleReferences, ProcessData processData)
        {
            XmlNode xnPurchaseOrderReferences = xnScheduleReferences.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_PURCHASE_ORDER_REFERENCE, "04", processData, processData.ScheduleID, "GetPurchaseOrderReference");
            if (xnPurchaseOrderReferences != null)
            {
                XmlNodeList xnlPurchaseOrderReferences = xnPurchaseOrderReferences.ChildNodes;
                for (int iPurchaseOrderIndex = 0; iPurchaseOrderIndex < xnlPurchaseOrderReferences.Count; iPurchaseOrderIndex++)
                {
                    if (xnlPurchaseOrderReferences[iPurchaseOrderIndex].Name.Contains(MeridianGlobalConstants.XCBL_SELLER_ORDER_NUMBER))
                    {
                        processData.OrderNumber = xnlPurchaseOrderReferences[iPurchaseOrderIndex].InnerText.ReplaceSpecialCharsWithSpace();
                        processData.ShippingSchedule.OrderNumber = processData.OrderNumber;

                        string formattedOrderNumber = processData.OrderNumber.ReplaceSpecialCharsWithSpace().Replace(" ", "");
                        string fileNameFormat = DateTime.Now.ToString(MeridianGlobalConstants.XCBL_FILE_DATETIME_FORMAT);
                        processData.CsvFileName = string.Concat(MeridianGlobalConstants.XCBL_AWC_FILE_PREFIX, fileNameFormat, formattedOrderNumber, MeridianGlobalConstants.XCBL_FILE_EXTENSION);
                        processData.XmlFileName = string.Concat(MeridianGlobalConstants.XCBL_AWC_FILE_PREFIX, fileNameFormat, formattedOrderNumber, MeridianGlobalConstants.XCBL_XML_EXTENSION);
                    }

                    if (xnlPurchaseOrderReferences[iPurchaseOrderIndex].Name.Contains(MeridianGlobalConstants.XCBL_ORDER_TYPE))
                    {
                        XmlNodeList xnlOrderType = xnlPurchaseOrderReferences[iPurchaseOrderIndex].ChildNodes;
                        for (int iOrderType = 0; iOrderType < xnlOrderType.Count; iOrderType++)
                        {
                            if (xnlOrderType[iOrderType].Name.Contains(MeridianGlobalConstants.XCBL_ORDER_TYPE_CODED_OTHER))
                                processData.ShippingSchedule.OrderType = xnlOrderType[iOrderType].InnerText.ReplaceSpecialCharsWithSpace();
                        }
                    }

                    if (xnlPurchaseOrderReferences[iPurchaseOrderIndex].Name.Contains(MeridianGlobalConstants.XCBL_CHANGE_ORDER_SEQUENCE_NUMBER))
                        processData.ShippingSchedule.SequenceNumber = xnlPurchaseOrderReferences[iPurchaseOrderIndex].InnerText.ReplaceSpecialCharsWithSpace();
                }
            }
        }

        private void GetOtherScheduleReferences(XmlNamespaceManager xmlNsManager, XmlNode xnScheduleReferences, ProcessData processData)
        {
            XmlNode xnOtherScheduleReferences = xnScheduleReferences.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_OTHER_SCHEDULE_REFERENCES, "05", processData, processData.ScheduleID, "GetOtherScheduleReferences");
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
                        processData.ShippingSchedule.SetOtherScheduleReferenceDesc(xnReferences[1].InnerText, xnReferences[2].InnerText.ReplaceSpecialCharsWithSpace());
                }
            }
        }

        private void GetPurposeScheduleTypeCodeAndParty(XmlNamespaceManager xmlNsManager, XmlNode element, ProcessData processData)
        {
            string methodName = "GetPurposeScheduleTypeCodeAndParty";
            XmlNode purposeCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_PURPOSE_CODED, "03", processData, processData.ScheduleID, methodName);
            if (purposeCoded != null)
                processData.ShippingSchedule.PurposeCoded = purposeCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode scheduleTypeCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SCHEDULE_TYPE_CODED, "04", processData, processData.ScheduleID, methodName);
            if (scheduleTypeCoded != null)
                processData.ShippingSchedule.ScheduleType = scheduleTypeCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode agencyCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_AGENCY_CODED, "05", processData, processData.ScheduleID, methodName);
            if (agencyCoded != null)
                processData.ShippingSchedule.AgencyCoded = agencyCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode name1 = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_NAME, "06", processData, processData.ScheduleID, methodName);
            if (name1 != null)
                processData.ShippingSchedule.Name1 = name1.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode street = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_STREET, "07", processData, processData.ScheduleID, methodName);
            if (street != null)
                processData.ShippingSchedule.Street = street.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode streetSupplement1 = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_STREET_SUPPLEMENT, "08", processData, processData.ScheduleID, methodName);
            if (streetSupplement1 != null)
                processData.ShippingSchedule.StreetSupplement1 = streetSupplement1.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode postalCode = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_POSTAL_CODE, "09", processData, processData.ScheduleID, methodName);
            if (postalCode != null)
                processData.ShippingSchedule.PostalCode = postalCode.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode city = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_CITY, "10", processData, processData.ScheduleID, methodName);
            if (city != null)
                processData.ShippingSchedule.City = city.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode regionCoded = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_REGION_CODED, "11", processData, processData.ScheduleID, methodName);
            if (regionCoded != null)
                processData.ShippingSchedule.RegionCoded = regionCoded.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode contactName = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_CONTACT_NAME, "12", processData, processData.ScheduleID, methodName);
            if (contactName != null)
                processData.ShippingSchedule.ContactName = contactName.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();
        }

        private void GetListOfContactNumber(XmlNamespaceManager xmlNsManager, XmlNode element, ProcessData processData)
        {
            var lisOfContactNumber = element.GetNodeByNameAndLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LIST_OF_CONTACT_NUMBERS, "13", processData, processData.ScheduleID, "GetListOfContactNumber");
            if (lisOfContactNumber != null && lisOfContactNumber.ChildNodes != null)
            {
                XmlNodeList xnlContactNames = lisOfContactNumber.ChildNodes;
                for (int iContactNameIndex = 0; iContactNameIndex < xnlContactNames.Count; iContactNameIndex++)
                {
                    XmlNodeList xnlContactValues = xnlContactNames[iContactNameIndex].ChildNodes;
                    for (int iContactValuesIndex = 0; iContactValuesIndex < xnlContactValues.Count; iContactValuesIndex++)
                        if (xnlContactValues[iContactValuesIndex].Name.Contains(MeridianGlobalConstants.XCBL_CONTACT_VALUE))
                            processData.ShippingSchedule.SetContactNumbers(xnlContactValues[iContactValuesIndex].InnerText.ReplaceSpecialCharsWithSpace(), iContactNameIndex);
                }
            }
            //else if (lisOfContactNumber.ChildNodes == null)
                //MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "GetListOfContactNumber", "14", "Warning - The Contact Number Not Found.", "Warning - Contact Number", processData.CsvFileName, processData.ScheduleID, processData.OrderNumber, null, "Warning 14 - Contact Name Not Found");

        }

        private void GetListOfTransportRouting(XmlNamespaceManager xmlNsManager, XmlNode element, ProcessData processData)
        {

            XmlNode shippingInstruction = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_SHIPPING_INSTRUCTIONS, "15", processData, processData.ScheduleID);
            if (shippingInstruction != null)
                processData.ShippingSchedule.ShippingInstruction = shippingInstruction.InnerText.Replace(",", "").ReplaceSpecialCharsWithSpace();

            XmlNode gpsSystem = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_GPS_SYSTEM, "16", processData, processData.ScheduleID);
            if (gpsSystem != null)
                processData.ShippingSchedule.GPSSystem = gpsSystem.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode latitude = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LATITUDE, "17", processData, processData.ScheduleID);
            if (latitude != null)
            {
                double dLatitude;
                double.TryParse(latitude.InnerText.ReplaceSpecialCharsWithSpace(), out dLatitude);
                processData.ShippingSchedule.Latitude = dLatitude;

            }

            XmlNode longitude = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LONGITUDE, "18", processData, processData.ScheduleID);
            if (longitude != null)
            {
                double dLongitude;
                double.TryParse(longitude.InnerText.ReplaceSpecialCharsWithSpace(), out dLongitude);
                processData.ShippingSchedule.Longitude = dLongitude;
            }

            XmlNode locationID = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_LOCATION_ID, "19", processData, processData.ScheduleID);
            if (locationID != null)
                processData.ShippingSchedule.LocationID = locationID.InnerText.ReplaceSpecialCharsWithSpace();

            XmlNode estimatedArrivalDate = element.GetNodeByNameAndInnerTextLogWarningTrans(xmlNsManager, MeridianGlobalConstants.XCBL_ESTIMATED_ARRIVAL_DATE, "20", processData, processData.ScheduleID);
            if (estimatedArrivalDate != null)
                processData.ShippingSchedule.EstimatedArrivalDate = estimatedArrivalDate.InnerText.ReplaceSpecialCharsWithSpace();

        }

        private bool UsePBSServiceDataAndUpdateFlags(ProcessData processData)
        {
            bool result = false;
            try
            {
                string destinationName = null;
                string orderNumber = null;
                string destinationStreet = null;
                string destinationStreetSupplement1 = null;
                string destinationPostalCode = null;
                string destinationCity = null;
                string destinationRegionCoded = null;
                string scheduledShipmentDateInString = null;
                string scheduledDeliveryDateInString = null;
                string isScheduled = null;

                /* Expecting data to come in below sequence and updating the fields based on this only
                 * 
                 * JobNo,Delivery Date,ShpDate,Scheduled,Order Date,Job Order,Job Category,Job Type,Customer,
                 * Company Name,Contract Number,Order Number,Origin Location,Origin Name,Origin Address,Origin Address2,
                 * Origin City,Origin State,Origin Zip,Origin Attention,Origin Phone,Origin Phone2,Origin Fax,
                 * Origin Email,Destination Location,Destination Name,Destination Address,Destination Address2,
                 * Destination City,Destination State,Destination Zip,Destination Attention,Destination Phone,
                 * Destination Phone2,Destination Email,Destination Note,Service Type,Mode,Partner Name,Driver Number,
                 * Driver,ApprDel,ShprNo
                 * 
                 */

                var pbsQueryResult = ProcessPBSQueryResult.Instance;
                var currentOrderDetails = new PBSData();
                if (pbsQueryResult.AllPBSOrder.ContainsKey(processData.OrderNumber.Trim()))
                    currentOrderDetails = pbsQueryResult.AllPBSOrder[processData.OrderNumber.Trim()];
                destinationName = currentOrderDetails.DestinationName;
                orderNumber = currentOrderDetails.OrderNumber;
                destinationStreet = currentOrderDetails.DestinationStreet;
                destinationStreetSupplement1 = currentOrderDetails.DestinationStreetSupplyment1;
                destinationPostalCode = currentOrderDetails.DestinationPostalCode;
                destinationCity = currentOrderDetails.DestinationCity;
                destinationRegionCoded = currentOrderDetails.DestinationRegionCoded;
                scheduledShipmentDateInString = currentOrderDetails.ShipmentDate;
                scheduledDeliveryDateInString = currentOrderDetails.DeliveryDate;
                isScheduled = currentOrderDetails.IsScheduled;

                if (!string.IsNullOrWhiteSpace(scheduledShipmentDateInString) && !string.IsNullOrWhiteSpace(orderNumber) && (processData.ShippingSchedule.OrderNumber.Trim().Equals(orderNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
                {

                    #region XCBL Data

                    var xcblRequestDate = DateTimeOffset.Parse(processData.ShippingSchedule.ScheduleIssuedDate).UtcDateTime;
                    var xcblScheduledShipDate = DateTimeOffset.Parse(processData.ShippingSchedule.ScheduleIssuedDate).UtcDateTime;
                    DateTime? xcblScheduledDeliveryDate = null;
                    if (!string.IsNullOrWhiteSpace(processData.ShippingSchedule.EstimatedArrivalDate))
                        xcblScheduledDeliveryDate = DateTimeOffset.Parse(processData.ShippingSchedule.EstimatedArrivalDate).UtcDateTime;

                    var xcblDeliveryName = processData.ShippingSchedule.Name1 ?? "";
                    var xcblStreet = processData.ShippingSchedule.Street ?? "";
                    var xcblStreetSupplement1 = processData.ShippingSchedule.StreetSupplement1 ?? "";
                    var xcblPostalCode = processData.ShippingSchedule.PostalCode ?? "";
                    var xcblCity = processData.ShippingSchedule.City ?? "";
                    var xcblRegionCoded = processData.ShippingSchedule.RegionCoded ?? "";

                    var xcblSameDay = processData.ShippingSchedule.Other_SameDay ?? "";
                    var xcblFirstStop = processData.ShippingSchedule.Other_FirstStop ?? "";
                    var xcblBefore7 = processData.ShippingSchedule.Other_Before7 ?? "";
                    var xcblBefore9 = processData.ShippingSchedule.Other_Before9 ?? "";
                    var xcblBefore12 = processData.ShippingSchedule.Other_Before12 ?? "";
                    var xcblOwnerOccupied = processData.ShippingSchedule.Other_OwnerOccupied ?? "";

                    #endregion  XCBL Data

                    #region PBS Data

                    destinationName = destinationName ?? "";
                    destinationStreet = destinationStreet ?? "";
                    destinationStreetSupplement1 = destinationStreetSupplement1 ?? "";
                    destinationCity = destinationCity ?? "";
                    destinationPostalCode = processData.ShippingSchedule.PostalCode ?? "";
                    destinationRegionCoded = destinationRegionCoded ?? "";
                    isScheduled = isScheduled ?? "";

                    var scheduledShipmentDate = DateTimeOffset.Parse(scheduledShipmentDateInString).UtcDateTime;
                    var scheduledShipmentDate10AM = new DateTime(scheduledShipmentDate.Year, scheduledShipmentDate.Month, scheduledShipmentDate.Day, 10, 0, 0);

                    DateTime? scheduledDeliveryDate = null;
                    DateTime? scheduledDeliveryDate10AM = null;
                    if (!string.IsNullOrWhiteSpace(scheduledDeliveryDateInString))
                        scheduledDeliveryDate = DateTimeOffset.Parse(scheduledDeliveryDateInString).UtcDateTime;
                    if (!string.IsNullOrWhiteSpace(scheduledDeliveryDateInString))
                        scheduledDeliveryDate10AM = new DateTime(scheduledDeliveryDate.Value.Year, scheduledDeliveryDate.Value.Month, scheduledDeliveryDate.Value.Day, 10, 0, 0);

                    #endregion PBS Data

                    var currentDateTime = DateTime.UtcNow;

                    if (isScheduled.Trim().Equals(MeridianGlobalConstants.PBS_SCHEDULED_FALSE, StringComparison.OrdinalIgnoreCase))
                    {
                        processData.ShippingSchedule.Approve05 = _meridianResult.Approve05 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if ((scheduledShipmentDate >= currentDateTime) && (xcblRequestDate > scheduledShipmentDate10AM.AddDays(-2)))
                    {
                        processData.ShippingSchedule.Pending01 = _meridianResult.Pending01 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if ((scheduledShipmentDate < currentDateTime) && scheduledDeliveryDate10AM.HasValue && (xcblRequestDate > scheduledDeliveryDate10AM.Value.AddDays(-2)))
                    {
                        processData.ShippingSchedule.Pending02 = _meridianResult.Pending02 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if (xcblSameDay.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase) ||
                        xcblFirstStop.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase) ||
                        xcblBefore7.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase) ||
                        xcblBefore9.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase) ||
                        xcblBefore12.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase) ||
                        xcblOwnerOccupied.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase))
                    {
                        processData.ShippingSchedule.Pending03 = _meridianResult.Pending03 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if (!xcblStreet.Trim().Equals(destinationStreet.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        !xcblStreetSupplement1.Trim().Equals(destinationStreetSupplement1.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        !xcblPostalCode.Trim().Equals(destinationPostalCode.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        !xcblCity.Trim().Equals(destinationCity.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        !xcblRegionCoded.Trim().Equals(destinationRegionCoded.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        processData.ShippingSchedule.Pending04 = _meridianResult.Pending04 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if ((processData.ShippingSchedule.OrderType != null) &&
                        (processData.ShippingSchedule.OrderType.Trim().Equals(MeridianGlobalConstants.XCBL_ORDER_TYPE_NPT, StringComparison.OrdinalIgnoreCase)
                         || processData.ShippingSchedule.OrderType.Trim().Equals(MeridianGlobalConstants.XCBL_ORDER_TYPE_RRO, StringComparison.OrdinalIgnoreCase)))
                    {
                        processData.ShippingSchedule.Approve01 = _meridianResult.Approve01 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if ((scheduledShipmentDate > currentDateTime) && (xcblRequestDate <= scheduledShipmentDate10AM.AddDays(-2)))
                    {
                        processData.ShippingSchedule.Approve02 = _meridianResult.Approve02 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if ((scheduledShipmentDate < currentDateTime) && scheduledDeliveryDate10AM.HasValue && (xcblRequestDate <= scheduledDeliveryDate10AM.Value.AddDays(-2)))
                    {
                        processData.ShippingSchedule.Approve03 = _meridianResult.Approve03 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else if ((xcblScheduledShipDate == scheduledShipmentDate) &&
                        xcblScheduledDeliveryDate.HasValue && scheduledDeliveryDate.HasValue &&
                        (xcblScheduledDeliveryDate.Value == scheduledDeliveryDate.Value) &&
                        xcblSameDay.Equals(MeridianGlobalConstants.XCBL_NO_FLAG, StringComparison.OrdinalIgnoreCase) &&
                        xcblFirstStop.Equals(MeridianGlobalConstants.XCBL_NO_FLAG, StringComparison.OrdinalIgnoreCase) &&
                        xcblBefore7.Equals(MeridianGlobalConstants.XCBL_NO_FLAG, StringComparison.OrdinalIgnoreCase) &&
                        xcblBefore9.Equals(MeridianGlobalConstants.XCBL_NO_FLAG, StringComparison.OrdinalIgnoreCase) &&
                        xcblBefore12.Equals(MeridianGlobalConstants.XCBL_NO_FLAG, StringComparison.OrdinalIgnoreCase) &&
                        xcblOwnerOccupied.Equals(MeridianGlobalConstants.XCBL_NO_FLAG, StringComparison.OrdinalIgnoreCase))
                    {
                        processData.ShippingSchedule.Approve04 = _meridianResult.Approve04 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                    else
                    {
                        processData.ShippingSchedule.Pending05 = _meridianResult.Pending05 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    }
                }
                else
                {
                    processData.ShippingSchedule.Rejected01 = _meridianResult.Rejected01 = MeridianGlobalConstants.XCBL_YES_FLAG;
                    processData.ShippingSchedule.Comments = _meridianResult.Comments = MeridianGlobalConstants.XCBL_COMMENT_ORDER_NOT_FOUND;
                   // MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "UsePBSServiceDataAndUpdateFlags", "02.24", "Warning - No Data from PBS WebService", string.Format("Warning - No data got for Order '{0}' from PBS WebService", processData.OrderNumber), processData.CsvFileName, processData.ScheduleID, processData.OrderNumber, null, "Warning 02.24");
                }

                result = true;
                MeridianSystemLibrary.LogPBS(
                    processData.ScheduleID, processData.OrderNumber,
                    processData.ShippingSchedule.Approve01, processData.ShippingSchedule.Approve02,
                    processData.ShippingSchedule.Approve03, processData.ShippingSchedule.Approve04,
                    processData.ShippingSchedule.Approve05, processData.ShippingSchedule.Pending01,
                    processData.ShippingSchedule.Pending02, processData.ShippingSchedule.Pending03,
                    processData.ShippingSchedule.Pending04, processData.ShippingSchedule.Pending05,
                    "ShippingSchedule", processData.ShippingSchedule.Rejected01, processData.ShippingSchedule.Comments);
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex);
                ApplicationError.logErrors(ex, "UsePBSServiceDataAndUpdateFlags");
            }

            return result;
        }

        private bool CreateLocalCsvFile(ProcessData processData)
        {
            bool result = false;
            try
            {
                if (processData != null && !string.IsNullOrEmpty(processData.ScheduleID)
                     && !string.IsNullOrEmpty(processData.OrderNumber)
                    && !string.IsNullOrEmpty(processData.CsvFileName))
                {
                    var initialResponse = (processData.ShippingSchedule.Approve01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve02.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve03.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve04.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve05.Equals(MeridianGlobalConstants.XCBL_YES_FLAG)) ?
                                           MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_REQUEST_ACCEPTED_FOR_CSV :
                                           processData.ShippingSchedule.Rejected01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ?
                                           MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_REQUEST_REJECTED_FOR_CSV :
                                           MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_REQUEST_PENDING_FOR_CSV;

                    var record = string.Format(MeridianGlobalConstants.CSV_HEADER_NAMES_FORMAT,
                       processData.ShippingSchedule.ScheduleID, processData.ShippingSchedule.ScheduleIssuedDate, processData.ShippingSchedule.OrderNumber, processData.ShippingSchedule.SequenceNumber,
                       processData.ShippingSchedule.Other_FirstStop, processData.ShippingSchedule.Other_Before7, processData.ShippingSchedule.Other_Before9, processData.ShippingSchedule.Other_Before12, processData.ShippingSchedule.Other_SameDay, processData.ShippingSchedule.Other_OwnerOccupied, processData.ShippingSchedule.Other_7, processData.ShippingSchedule.Other_8, processData.ShippingSchedule.Other_9, processData.ShippingSchedule.Other_10,
                       processData.ShippingSchedule.PurposeCoded, processData.ShippingSchedule.ScheduleType, processData.ShippingSchedule.AgencyCoded, processData.ShippingSchedule.Name1, processData.ShippingSchedule.Street, processData.ShippingSchedule.StreetSupplement1, processData.ShippingSchedule.PostalCode, processData.ShippingSchedule.City, processData.ShippingSchedule.RegionCoded,
                       processData.ShippingSchedule.ContactName, processData.ShippingSchedule.ContactNumber_1, processData.ShippingSchedule.ContactNumber_2, processData.ShippingSchedule.ContactNumber_3, processData.ShippingSchedule.ContactNumber_4, processData.ShippingSchedule.ContactNumber_5, processData.ShippingSchedule.ContactNumber_6,
                       processData.ShippingSchedule.ShippingInstruction, processData.ShippingSchedule.GPSSystem, processData.ShippingSchedule.Latitude.ToString(), processData.ShippingSchedule.Longitude.ToString(), processData.ShippingSchedule.LocationID, processData.ShippingSchedule.EstimatedArrivalDate, processData.ShippingSchedule.OrderType, initialResponse);
                    StringBuilder strBuilder = new StringBuilder(MeridianGlobalConstants.CSV_HEADER_NAMES);
                    strBuilder.AppendLine();
                    strBuilder.AppendLine(record);
                    string csvContent = strBuilder.ToString();
                   int exeresult=  MeridianSystemLibrary.UpdateDataToLocal(processData);
                    if (exeresult > 0)
                        result = true;
                    else
                        result = false;
                    _meridianResult.FtpUserName = processData.FtpUserName;
                    _meridianResult.FtpPassword = processData.FtpPassword;
                    _meridianResult.FtpServerInFolderPath = processData.FtpServerInFolderPath;
                    _meridianResult.FtpServerOutFolderPath = processData.FtpServerOutFolderPath;
                    _meridianResult.LocalFilePath = processData.LocalFilePath;
                    _meridianResult.WebUserName = processData.WebUserName;
                    _meridianResult.UniqueID = processData.ScheduleID;
                    _meridianResult.OrderNumber = processData.OrderNumber;
                    _meridianResult.FileName = processData.CsvFileName;

                    //if (MeridianGlobalConstants.CONFIG_CREATE_LOCAL_CSV == MeridianGlobalConstants.SHOULD_CREATE_LOCAL_FILE)
                    //{
                    //    _meridianResult.UploadFromLocalPath = true;
                    //    return CommonProcess.CreateFile(csvContent, _meridianResult);
                    //}
                    //else
                    //{
                    //    byte[] content = Encoding.UTF8.GetBytes(csvContent);
                    //    int length = content.Length;

                    //    if (!string.IsNullOrEmpty(processData.CsvFileName) && length > 40)
                    //    {
                    //        _meridianResult.Content = content;
                    //        result = true;
                    //    }
                    //    else
                    //    {
                    //       // MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "CreateLocalCsvFile", "03.06", ("Error - Creating CSV File because of Stream " + length), string.Format("Error - Creating CSV File {0} with error of Stream", processData.CsvFileName), processData.CsvFileName, processData.ScheduleID, processData.OrderNumber, processData.XmlDocument, "Error 03.06- Create CSV");
                    //    }
                    //}
                }
                else
                {
                   // MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "CreateLocalCsvFile", "03.06", "Error - Creating CSV File because of Process DATA", string.Format("Error - Creating CSV File {0} with error of Process DATA", processData.CsvFileName), processData.CsvFileName, processData.ScheduleID, processData.OrderNumber, processData.XmlDocument, "Error 03.06- Create CSV");
                }
            }
            catch (Exception ex)
            {
                //MeridianSystemLibrary.LogTransaction(processData.WebUserName, processData.FtpUserName, "CreateLocalCsvFile", "03.06", "Error - Creating CSV File", string.Format("Error - Creating CSV File {0} with error {1}", processData.CsvFileName, ex.Message), processData.CsvFileName, processData.ScheduleID, processData.OrderNumber, processData.XmlDocument, "Error 03.06- Create CSV");
            }

            return result;
        }

        private bool UpdateToServer(ProcessData processData)
        {
            bool result = false;
            try
            {
                if (processData != null && !string.IsNullOrEmpty(processData.ScheduleID)
                     && !string.IsNullOrEmpty(processData.OrderNumber)
                    && !string.IsNullOrEmpty(processData.CsvFileName))
                {
                    var initialResponse = (processData.ShippingSchedule.Approve01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve02.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve03.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve04.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ||
                                           processData.ShippingSchedule.Approve05.Equals(MeridianGlobalConstants.XCBL_YES_FLAG)) ?
                                           MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_REQUEST_ACCEPTED_FOR_CSV :
                                           processData.ShippingSchedule.Rejected01.Equals(MeridianGlobalConstants.XCBL_YES_FLAG) ?
                                           MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_REQUEST_REJECTED_FOR_CSV :
                                           MeridianGlobalConstants.XCBL_SHIPPING_SCHEDULE_REQUEST_PENDING_FOR_CSV;

                    var record = string.Format(MeridianGlobalConstants.CSV_HEADER_NAMES_FORMAT,
                       processData.ShippingSchedule.ScheduleID, processData.ShippingSchedule.ScheduleIssuedDate, processData.ShippingSchedule.OrderNumber, processData.ShippingSchedule.SequenceNumber,
                       processData.ShippingSchedule.Other_FirstStop, processData.ShippingSchedule.Other_Before7, processData.ShippingSchedule.Other_Before9, processData.ShippingSchedule.Other_Before12, processData.ShippingSchedule.Other_SameDay, processData.ShippingSchedule.Other_OwnerOccupied, processData.ShippingSchedule.Other_7, processData.ShippingSchedule.Other_8, processData.ShippingSchedule.Other_9, processData.ShippingSchedule.Other_10,
                       processData.ShippingSchedule.PurposeCoded, processData.ShippingSchedule.ScheduleType, processData.ShippingSchedule.AgencyCoded, processData.ShippingSchedule.Name1, processData.ShippingSchedule.Street, processData.ShippingSchedule.StreetSupplement1, processData.ShippingSchedule.PostalCode, processData.ShippingSchedule.City, processData.ShippingSchedule.RegionCoded,
                       processData.ShippingSchedule.ContactName, processData.ShippingSchedule.ContactNumber_1, processData.ShippingSchedule.ContactNumber_2, processData.ShippingSchedule.ContactNumber_3, processData.ShippingSchedule.ContactNumber_4, processData.ShippingSchedule.ContactNumber_5, processData.ShippingSchedule.ContactNumber_6,
                       processData.ShippingSchedule.ShippingInstruction, processData.ShippingSchedule.GPSSystem, processData.ShippingSchedule.Latitude.ToString(), processData.ShippingSchedule.Longitude.ToString(), processData.ShippingSchedule.LocationID, processData.ShippingSchedule.EstimatedArrivalDate, processData.ShippingSchedule.OrderType, initialResponse);
                    // StringBuilder strBuilder = new StringBuilder(MeridianGlobalConstants.CSV_HEADER_NAMES);
                    // strBuilder.AppendLine();
                    // strBuilder.AppendLine(record);
                    // string csvContent = strBuilder.ToString();

                    DataSet ds = MeridianSystemLibrary.GetDataByRef(processData.ShippingSchedule.OrderNumber);
                    if(ds.Tables[0].Rows.Count >0)
                    {
                        if(ds.Tables[0].Rows[0]["CANCELLED"].ToString() == "1")
                        {
                            ApplicationError.WriteMessage("Ref Number Order Canceled : " + processData.ShippingSchedule.OrderNumber.Replace(" ", "") + " Order Canceled Failure Notify.  ");
                            return result = false;
                        }
                        if(ds.Tables[0].Rows[0]["DROPDATE"] != DBNull.Value && ds.Tables[0].Rows[0]["DROPPEDOFF"].ToString() =="1")
                        {
                            ApplicationError.WriteMessage("Ref Number :" + processData.ShippingSchedule.OrderNumber.Replace(" ", "") + " All Ready Deliverd Failure Notify.   ");
                            return result = false;
                        }                         
                    }
                    else
                    {
                        ApplicationError.WriteMessage("Ref Number Not Found : " + processData.ShippingSchedule.OrderNumber.Replace(" ",""));
                        return result = false;
                    }

                    int exeresult = MeridianSystemLibrary.UpdateDataToLocal(processData);
                    if (exeresult > 0)
                    {
                        ApplicationError.WriteMessage("Success Process Ref Number : " + processData.ShippingSchedule.OrderNumber);
                        result = true;
                    }                        
                    else
                        result = false;
                     
                }
                else
                {
                     
                }
            }
            catch (Exception ex)
            {
                ApplicationError.logErrors(ex, "UpdateToServer");
            }

            return result;
        }
    }
}