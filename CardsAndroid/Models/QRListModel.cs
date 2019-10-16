using System;
using Android.Graphics;
using CardsPCL.Models;

namespace CardsAndroid.Models
{
    public class QrListModel
    {
        public Bitmap QrImage { get; set; }
        public Bitmap LogoImage { get; set; }
        public string CardName { get; set; }
        //public int CardId { get; set; }
        public SubscriptionModel Subscription { get; set; }
        public CompanyWithNotPublicLogo Company { get; set; }
        public Person Person { get; set; }
        public string Culture { get; set; }
        public string Url { get; set; }
        public bool IsPublic { get; set; }
        public bool IsPrimary { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
