using System.Collections.Generic;

namespace WebAPI.model
{
    public class Oauth
    {
        public int MID { get; set; }
        public string PassportCode { get; set; }
    }

    public class MemberInfo
    {
        public int OID { get; set; }
        public string CName { get; set; }
        public string CDes { get; set; }
        public string Since { get; set; }
        public string EMail { get; set; }
        public string Address { get; set; }
        public string Birthday { get; set; }
        public string AvatarURL { get; set; }
    }

    public class UpdateMemberInfo
    {
        public string cName { get; set; }
        public string cDes { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string birthday { get; set; }
        public int aid { get; set; }
        public bool changeAvatar { get; set; }
    }

    public class MemberAvatar
    {
        public int AID { get; set; }
        public string UUID { get; set; }
    }
}
