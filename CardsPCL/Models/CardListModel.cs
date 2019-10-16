using System;
namespace CardsPCL.Models
{
    public class CardListModel
    {
        public SubscriptionModel subscription { get; set; }
        public CompanyWithNotPublicLogo company { get; set; }
        public string culture { get; set; }
        public string url { get; set; }
        public bool isPublic { get; set; }
        public bool isPrimary { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }
}
