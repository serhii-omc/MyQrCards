using System;
using System.Collections.Generic;

namespace CardsPCL.Models
{
    public class AuthorizeRootObject
    {
        public int? accountID { get; set; }
        public string email { get; set; }
        public string accountUrl { get; set; }
        public bool isVerified { get; set; }
        public List<Subscription> subscriptions { get; set; }
        public string accessJwt { get; set; }
    }
}
