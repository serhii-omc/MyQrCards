using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("LoginedFromTable")]
    public class LoginedFromTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string logined_from_path { get; set; }
    }
}
