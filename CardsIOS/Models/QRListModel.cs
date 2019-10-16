using System;
using CardsPCL.Models;
using UIKit;

namespace CardsIOS.Models
{
    public class QRListModel
    {
        public UIImage QRImage { get; set; }
        public UIImage LogoImage { get; set; }
        public bool isLogoStandard { get; set; }
        public string CardName { get; set; }
        //public int CardId { get; set; }
        public SubscriptionModel subscription { get; set; }
        public CompanyWithNotPublicLogo company { get; set; }
        public Person person { get; set; }
        public string culture { get; set; }
        public string url { get; set; }
        public bool isPublic { get; set; }
        public bool isPrimary { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }
}
