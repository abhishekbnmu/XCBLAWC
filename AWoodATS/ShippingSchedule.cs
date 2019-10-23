using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AWoodATS
{
    public class ShippingSchedule
    {
        public ShippingSchedule()
        {
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

        public string ScheduleID { get; set; }
        public string ScheduleIssuedDate { get; set; }
        public string OrderNumber { get; set; }
        public string OrderType { get; set; }
        public string SequenceNumber { get; set; }
        public string Other_FirstStop { get; set; }
        public string Other_Before7 { get; set; }
        public string Other_Before9 { get; set; }
        public string Other_Before12 { get; set; }
        public string Other_SameDay { get; set; }
        public string Other_OwnerOccupied { get; set; }
        public string Other_7 { get; set; }
        public string Other_8 { get; set; }
        public string Other_9 { get; set; }
        public string Other_10 { get; set; }
        public string PurposeCoded { get; set; }
        public string ScheduleType { get; set; }
        public string AgencyCoded { get; set; }
        public string Name1 { get; set; }
        public string Street { get; set; }
        public string StreetSupplement1 { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string RegionCoded { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber_1 { get; set; }
        public string ContactNumber_2 { get; set; }
        public string ContactNumber_3 { get; set; }
        public string ContactNumber_4 { get; set; }
        public string ContactNumber_5 { get; set; }
        public string ContactNumber_6 { get; set; }
        public string ShippingInstruction { get; set; }
        public string GPSSystem { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationID { get; set; }
        public string EstimatedArrivalDate { get; set; }
        public string WorkOrderNumber { get; set; }
        public string SSID { get; set; }
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
