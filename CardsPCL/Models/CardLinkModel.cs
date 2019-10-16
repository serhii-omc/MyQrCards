using System;
namespace CardsPCL.Models
{
    public class CardLinkModel
    {
        public bool isPublic { get; set; }
        public bool isDefault { get; set; }
        public string url { get; set; }
        public Account account { get; set; }
        public Card card { get; set; }
    }
}
