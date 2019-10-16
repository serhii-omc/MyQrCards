using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    public class LoginAfterTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string accessJwt { get; set; }
        public string accountUrl { get; set; }
        public int subscriptionId { get; set; }
    }
}
