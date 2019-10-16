using System;
namespace CardsPCL.Models
{
    public class Company
    {
        public string phone { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string siteUrl { get; set; }
        public int? foundedYear { get; set; }
        public string activity { get; set; }
        public string customers { get; set; }
        public bool isReadOnly { get; set; }
        public CompanyLocation location { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        //public CompanyLogoModel logo { get; set; }
        public CompanyLogoNotPublicLogo logo { get; set; }
    }
}
