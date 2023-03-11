using System.Collections.Generic;

namespace WebAPI.model
{
    public class SpotComment
    {
        public int cid { get; set; }
        public int mid { get; set; }
        public string cName { get; set; }
        public string cDes { get; set; }
        public int star5 { get; set; }
        public int thumbUp { get; set; }
        public string create_date { get; set; }
        public bool like { get; set; }
        public string img { get; set; }
    }

    public class AddComment
    {
        public int oid { get; set; }
        public string cDes { get; set; }
        public int star5 { get; set; }
        public int[] imgs { get; set; }
    }
}
