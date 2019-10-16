using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("UsersCardTable")]
    public class UsersCardTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string surname { get; set; }
        public string name { get; set; }
        public string middlename { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string home_phone { get; set; }
        public string personal_site { get; set; }
        public string education { get; set; }
        public string card_name { get; set; }
        public string birthdate { get; set; }
        //public string home_address { get; set; }
        public string country { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string index { get; set; }
        public string notes { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public bool from_edit { get; set; }
    }
}
