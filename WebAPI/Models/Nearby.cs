using System.Collections.Generic;

namespace WebAPI.model
{
    public class Nearby
    {
        public int OID { get; set; }
        public string CName { get; set; }
        public int Type { get; set; }
        public string PictureUrl { get; set; }
        public double PositionLon { get; set; }
        public double PositionLat { get; set; }
    }
}
