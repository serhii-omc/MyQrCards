using SQLite;

namespace CardsPCL.Database.Tables
{
    [Table("CompanyCardTable")]
    public class CompanyCardTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string name { get; set; }
        public string activity { get; set; }
        public string position { get; set; }
        public string foundedYear { get; set; }
        public string customers { get; set; }
        public string companyPhone { get; set; }
        public string corporativePhone { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string siteUrl { get; set; }
        public string country { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string index { get; set; }
        public string notes { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string timestampUTC { get; set; }
    }
}
