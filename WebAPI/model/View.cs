namespace WebAPI.model
{
    public class View
    {
        public string view_name { get; set; }
        public string created { get; set; }
        public string last_modified { get; set; }
    }

    public class ViewDefinition
    {
        public string definition { get; set; }
    }

    public class ViewRelatedTable
    {

        public string view_name { get; set; }
        public string table_name { get; set; }
        public string entity_type { get; set; }
    }

    public class ViewColumn
    {
        public string view_name { get; set; }
        public int column_id { get; set; }
        public string column_name { get; set; }
        public string data_type { get; set; }
        public string max_length { get; set; }
        public int precision { get; set; }
    }

    public class ViewIndex
    {
        public string view_name { get; set; }
        public string index_name { get; set; }
        public string definition { get; set; }
    }
}
