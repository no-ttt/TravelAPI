namespace WebAPI.model
{
    public class Column
    {
        public string table_name { get; set; }
        public int column_id { get; set; }
        public string column_name { get; set; }
        public string data_type { get; set; }
        public string max_length { get; set; }
        public int precision { get; set; }
    }

    public class TabColumn
    {
        public int id { get; set; }
        public string name { get; set; }
        public string data_type { get; set; }
        public string max_length { get; set; }
        public int precision { get; set; }
        public string is_nullable { get; set; }

    }
}
