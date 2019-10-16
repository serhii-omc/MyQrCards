using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("DifferentPurposesTable")]
    public class DifferentPurposesTable
    {

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public bool show_map_hint { get; set; }
        //ATTENTION!!!!!! IF HERE WOULD BE ANOTHER FIELDS THEY WILL BE REMOVED WHILE INSERT
    }
}
