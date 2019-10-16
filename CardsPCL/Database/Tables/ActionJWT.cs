using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("ActionJWT")]
    public class ActionJWT
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string actionJwt { get; set; }
    }
}
