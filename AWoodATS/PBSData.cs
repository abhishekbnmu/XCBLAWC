using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AWoodATS
{
    public class PBSData
    {
        public string ScheduleId { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string DeliveryDate { get; set; }
        public string ShipmentDate { get; set; }
        public string DestinationName { get; set; }
        public string DestinationStreet { get; set; }
        public string DestinationStreetSupplyment1 { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationRegionCoded { get; set; }
        public string DestinationPostalCode { get; set; }
        public string IsScheduled { get; set; }
    }
}