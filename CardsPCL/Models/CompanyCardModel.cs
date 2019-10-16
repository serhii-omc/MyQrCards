using System;
using System.Collections.Generic;

namespace CardsPCL.Models
{
    public class CompanyCardModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string SiteUrl { get; set; }
        public int? LogoAttachmentID { get; set; }
        public int? FoundedYear { get; set; }
        public string Activity { get; set; }
        public string Customers { get; set; }
        public LocationModel Location { get; set; }
        public List<SocialNetworkModel> SocialNetworks { get; set; }
    }
}
