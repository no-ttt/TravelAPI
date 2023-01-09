using System;
using System.Collections.Generic;

namespace WebAPI.model
{
    public class Activity
    {
        public int oid { get; set; }
        public string area { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string cName { get; set; }
        public string cDes { get; set; }
        public string pictureUrl { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }

    public class ActivityDetail
    {
        public int oid { get; set; }
        public string cName { get; set; }
        public string cDes { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string location { get; set; }
        public string organizer { get; set; }
        public string particpation { get; set; }
        public string travelInfo { get; set; }
        public string pictureUrl { get; set; }
        public string parkingInfo { get; set; }
        public string charge { get; set; }
        public string remarks { get; set; }
        public string tags { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string cycle { get; set; }
        public double PositionLon { get; set; }
        public double PositionLat { get; set; }
    }
}
