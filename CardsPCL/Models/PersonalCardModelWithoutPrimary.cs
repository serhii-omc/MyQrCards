using System;
namespace CardsPCL.Models
{
    public class PersonalCardModelWithoutPrimary
    {
        public int SubscriptionID { get; set; }
        public string Name { get; set; }
        public string Culture { get; set; }
        public PersonModel Person { get; set; }
        public EmploymentModel Employment { get; set; }
    }
}
