using System;
namespace CardsPCL.Models
{
    public class PersonLocation
    {
        public int id { get; set; }
        public string country { get; set; }
        public string postalCode { get; set; }
        public string region { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string notes { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
    }
}
