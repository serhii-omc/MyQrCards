using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("ValidTill_RepeatAfterTable")]
    public class ValidTill_RepeatAfterTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public DateTime validTill { get; set; }
        public DateTime repeatAfter { get; set; }
        public string email { get; set; }
    }
}
