using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("PersonalSocialNetworkTable")]
    public class PersonalSocialNetworkTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public int SocialNetworkID { get; set; }
        public string ContactUrl { get; set; }
    }
}
