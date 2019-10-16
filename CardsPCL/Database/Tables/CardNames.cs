using System;
using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("CardNames")]
    public class CardNames
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string card_name { get; set; }
        public string card_url { get; set; }
        public int card_id { get; set; }
        public bool isLogoStandard { get; set; }
        public string PersonName { get; set; }
        public string PersonSurname { get; set; }
    }
}
