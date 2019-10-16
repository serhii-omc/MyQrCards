using System;
namespace CardsPCL.Models
{
    public class PersonalCardModel
    {
        public int? SubscriptionID { get; set; }
        public string Name { get; set; }
        public string Culture { get; set; }
        public bool IsPrimary { get; set; }
        public PersonModel Person { get; set; }
        public EmploymentModel Employment { get; set; }
    }
}
