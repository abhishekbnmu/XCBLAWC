using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 using System.Xml.Serialization;
 

namespace AWoodATS
{
    public class ShipingRequestXML
    {   
        [XmlRoot(ElementName = "OrderType", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class OrderType
        {
            [XmlElement(ElementName = "OrderTypeCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string OrderTypeCoded { get; set; }
            [XmlElement(ElementName = "OrderTypeCodedOther", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string OrderTypeCodedOther { get; set; }
        }

        [XmlRoot(ElementName = "PurchaseOrderReference", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class PurchaseOrderReference
        {
            [XmlElement(ElementName = "SellerOrderNumber", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string SellerOrderNumber { get; set; }
            [XmlElement(ElementName = "OrderType", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public OrderType OrderType { get; set; }
            [XmlElement(ElementName = "ChangeOrderSequenceNumber", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ChangeOrderSequenceNumber { get; set; }
        }

        [XmlRoot(ElementName = "ReferenceCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class ReferenceCoded
        {
            [XmlElement(ElementName = "ReferenceTypeCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ReferenceTypeCoded { get; set; }
            [XmlElement(ElementName = "ReferenceTypeCodedOther", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ReferenceTypeCodedOther { get; set; }
            [XmlElement(ElementName = "ReferenceDescription", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ReferenceDescription { get; set; }
        }

        [XmlRoot(ElementName = "OtherScheduleReferences", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class OtherScheduleReferences
        {
            [XmlElement(ElementName = "ReferenceCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public List<ReferenceCoded> ReferenceCoded { get; set; }
        }

        [XmlRoot(ElementName = "ScheduleReferences", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class ScheduleReferences
        {
            [XmlElement(ElementName = "PurchaseOrderReference", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public PurchaseOrderReference PurchaseOrderReference { get; set; }
            [XmlElement(ElementName = "OtherScheduleReferences", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public OtherScheduleReferences OtherScheduleReferences { get; set; }
        }

        [XmlRoot(ElementName = "Purpose", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class Purpose
        {
            [XmlElement(ElementName = "PurposeCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string PurposeCoded { get; set; }
        }

        [XmlRoot(ElementName = "Agency", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class Agency
        {
            [XmlElement(ElementName = "AgencyCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string AgencyCoded { get; set; }
        }

        [XmlRoot(ElementName = "PartyID", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class PartyID
        {
            [XmlElement(ElementName = "Agency", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public Agency Agency { get; set; }
            [XmlElement(ElementName = "Ident", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string Ident { get; set; }
        }

        [XmlRoot(ElementName = "Region", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class Region
        {
            [XmlElement(ElementName = "RegionCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string RegionCoded { get; set; }
        }

        [XmlRoot(ElementName = "NameAddress", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class NameAddress
        {
            [XmlElement(ElementName = "Name1", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string Name1 { get; set; }
            [XmlElement(ElementName = "Street", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string Street { get; set; }
            [XmlElement(ElementName = "StreetSupplement1", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string StreetSupplement1 { get; set; }
            [XmlElement(ElementName = "PostalCode", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string PostalCode { get; set; }
            [XmlElement(ElementName = "City", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string City { get; set; }
            [XmlElement(ElementName = "Region", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public Region Region { get; set; }
        }

        [XmlRoot(ElementName = "ContactNumber", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class ContactNumber
        {
            [XmlElement(ElementName = "ContactNumberValue", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ContactNumberValue { get; set; }
            [XmlElement(ElementName = "ContactNumberTypeCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ContactNumberTypeCoded { get; set; }
        }

        [XmlRoot(ElementName = "ListOfContactNumber", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class ListOfContactNumber
        {
            [XmlElement(ElementName = "ContactNumber", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public List<ContactNumber> ContactNumber { get; set; }
        }

        [XmlRoot(ElementName = "PrimaryContact", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class PrimaryContact
        {
            [XmlElement(ElementName = "ContactName", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ContactName { get; set; }
            [XmlElement(ElementName = "ListOfContactNumber", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public ListOfContactNumber ListOfContactNumber { get; set; }
        }

        [XmlRoot(ElementName = "ShipToParty", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class ShipToParty
        {
            [XmlElement(ElementName = "PartyID", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public PartyID PartyID { get; set; }
            [XmlElement(ElementName = "NameAddress", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public NameAddress NameAddress { get; set; }
            [XmlElement(ElementName = "PrimaryContact", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public PrimaryContact PrimaryContact { get; set; }
        }

        [XmlRoot(ElementName = "ScheduleParty", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class ScheduleParty
        {
            [XmlElement(ElementName = "ShipToParty", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public ShipToParty ShipToParty { get; set; }
        }

        [XmlRoot(ElementName = "GPSCoordinates", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class GPSCoordinates
        {
            [XmlElement(ElementName = "GPSSystem", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string GPSSystem { get; set; }
            [XmlElement(ElementName = "Latitude", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string Latitude { get; set; }
            [XmlElement(ElementName = "Longitude", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string Longitude { get; set; }
        }

        [XmlRoot(ElementName = "Location", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class Location
        {
            [XmlElement(ElementName = "GPSCoordinates", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public GPSCoordinates GPSCoordinates { get; set; }
        }

        [XmlRoot(ElementName = "EndTransportLocation", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class EndTransportLocation
        {
            [XmlElement(ElementName = "Location", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public Location Location { get; set; }
            [XmlElement(ElementName = "LocationID", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string LocationID { get; set; }
            [XmlElement(ElementName = "EstimatedArrivalDate", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string EstimatedArrivalDate { get; set; }
        }

        [XmlRoot(ElementName = "TransportLocationList", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class TransportLocationList
        {
            [XmlElement(ElementName = "EndTransportLocation", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public EndTransportLocation EndTransportLocation { get; set; }
        }

        [XmlRoot(ElementName = "TransportRouting", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
        public class TransportRouting
        {
            [XmlElement(ElementName = "ShippingInstructions", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public string ShippingInstructions { get; set; }
            [XmlElement(ElementName = "TransportLocationList", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public TransportLocationList TransportLocationList { get; set; }
        }

        [XmlRoot(ElementName = "ListOfTransportRouting", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class ListOfTransportRouting
        {
            [XmlElement(ElementName = "TransportRouting", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/core/core.xsd")]
            public TransportRouting TransportRouting { get; set; }
        }

        [XmlRoot(ElementName = "ShippingScheduleHeader", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class ShippingScheduleHeader
        {
            [XmlElement(ElementName = "ScheduleID", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public string ScheduleID { get; set; }
            [XmlElement(ElementName = "ScheduleIssuedDate", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public string ScheduleIssuedDate { get; set; }
            [XmlElement(ElementName = "ScheduleReferences", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public ScheduleReferences ScheduleReferences { get; set; }
            [XmlElement(ElementName = "Purpose", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public Purpose Purpose { get; set; }
            [XmlElement(ElementName = "ScheduleTypeCoded", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public string ScheduleTypeCoded { get; set; }
            [XmlElement(ElementName = "ScheduleParty", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public ScheduleParty ScheduleParty { get; set; }
            [XmlElement(ElementName = "ListOfTransportRouting", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public ListOfTransportRouting ListOfTransportRouting { get; set; }
        }

        [XmlRoot(ElementName = "ShippingSchedule", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
        public class ShippingSchedule
        {
            [XmlElement(ElementName = "ShippingScheduleHeader", Namespace = "rrn:org.xcbl:schemas/xcbl/v4_0/materialsmanagement/v1_0/materialsmanagement.xsd")]
            public ShippingScheduleHeader ShippingScheduleHeader { get; set; }
            [XmlAttribute(AttributeName = "core", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Core { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }

 

}
}