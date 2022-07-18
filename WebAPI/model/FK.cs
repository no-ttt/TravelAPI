namespace WebAPI.model
{
    public class FK
    {
        public string fk_constraint_name { get; set; }
        public int no { get; set; }
        public string foreign_table { get; set; }
        public string fk_column_name { get; set; }
        public string primary_table { get; set; }
        public string pk_column_name { get; set; }
    }
}
