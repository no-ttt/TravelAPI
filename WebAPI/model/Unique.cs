namespace WebAPI.model
{
    public class Unique
    {
        public string table_name { get; set; }
        public string key_name { get; set; }
        public string columns { get; set; }
        public string constraint_type { get; set; }
    }
}
