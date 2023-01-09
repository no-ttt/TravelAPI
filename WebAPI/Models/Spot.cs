using System;
using System.Collections.Generic;

namespace WebAPI.model
{
    public class AutoComplete
    {
        public string result { get; set; }
    }

    public class Spot
    {
        public int oid { get; set; }
        public int type { get; set; }
        public string cDes { get; set; }
        public string area { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string spotName { get; set; }
        public string pictureUrl { get; set; }
    }

    public class SpotDetail
    {
        public int oid { get; set; }
        public string cName { get; set; }
        public string cDes { get; set; }
        public int type { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string location { get; set; }
        public double positionLat { get; set; }
        public double positionLon { get; set; }
        public string websiteUrl { get; set; }
        public string travelInfo { get; set; }
        public string opentime { get; set; }
        public string pictureUrl { get; set; }
        public string ticketInfo { get; set; }
        public string parkingInfo { get; set; }
        public string serviceInfo { get; set; }
        public string spec { get; set; }
        public string grade { get; set; }
        public string charge { get; set; }
        public string tags { get; set; }
        public string remarks { get; set; }
        public string keyword { get; set; }
    }
}
