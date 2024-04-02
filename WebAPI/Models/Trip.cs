using System.Collections.Generic;

namespace WebAPI.model
{
    public class AddTrip
    {
        public string tripName { get; set; }
        public string remark { get; set; }
        public string startPos { get; set; }
        public string endPos { get; set; }
        public string startDate { get; set; }
        public string traffic { get; set; }
    }
    public class TripDatail
    {
        public string cName { get; set; }
        public string cDes { get; set; }
        public string since { get; set; }
        public string startPos { get; set; }
        public string endPos { get; set; }
        public string startDate { get; set; }
        public string dayNum { get; set; }
        public string traffic { get; set; }
        public string tripID { get; set; }
        public string ownerName { get; set; }
    }
    public class Trip
    {
        public int oid { get; set; }
        public string cName { get; set; }
        public string since { get; set; }
        public string startDate { get; set; }
        public string dayNum { get; set; }
    }
}
