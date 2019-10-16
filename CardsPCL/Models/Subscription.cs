using System;
using System.Collections.Generic;
using static CardsPCL.Models.AuthorizeModel;

namespace CardsPCL.Models
{
    public class Subscription
    {
        public Limitations limitations { get; set; }
        public List<string> features { get; set; }
        public DateTime validTill { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public bool isTrial { get; set; }
    }
}
