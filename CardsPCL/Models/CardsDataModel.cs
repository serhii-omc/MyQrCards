﻿using System;
using System.Collections.Generic;

namespace CardsPCL.Models
{
    public class CardsDataModel
    {
        public string accountUrl { get; set; }
        public DateTime created { get; set; }
        public Person person { get; set; }
        public Employment employment { get; set; }
        public List<CompanyLogoNotPublicLogo> gallery { get; set; }
        public string culture { get; set; }
        public string url { get; set; }
        public bool isPublic { get; set; }
        public bool isPrimary { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }
}
