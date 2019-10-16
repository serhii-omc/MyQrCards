using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("LastCloudSyncTable")]
    public class LastCloudSyncTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public DateTime? dateTime { get; set; }
    }
}
