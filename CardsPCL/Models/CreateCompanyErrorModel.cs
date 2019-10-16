using System;
namespace CardsPCL.Models
{
    public class CreateCompanyErrorModel
    {
        public string traceIdentifier { get; set; }
        public string code { get; set; }
        public string message { get; set; }
    }
}
