using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("ETag")]
    public class ETag
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string eTag { get; set; }
    }
}
