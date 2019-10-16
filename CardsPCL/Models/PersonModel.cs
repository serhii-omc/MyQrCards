using System;
using System.Collections.Generic;

namespace CardsPCL.Models
{
    public class PersonModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        public string AddressForm { get; set; }
        public string Email { get; set; }
        public string SiteUrl { get; set; }
        public string Education { get; set; }
        public string BirthDate { get; set; }
        public List<int> Attachments { get; set; }
        public LocationModel Location { get; set; }
        public List<SocialNetworkModel> SocialNetworks { get; set; }
    }
}
