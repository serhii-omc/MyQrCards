using System;
using System.Collections.Generic;

namespace CardsPCL.Models
{
    public class Person
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string mobilePhone { get; set; }
        public string homePhone { get; set; }
        public string addressForm { get; set; }
        public string email { get; set; }
        public string siteUrl { get; set; }
        public string education { get; set; }
        public string birthdate { get; set; }
        public List<SocialNetwork> socialNetworks { get; set; }
        public PersonLocation location { get; set; }
    }
}
