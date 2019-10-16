using System;
namespace CardsPCL.Models
{
    public class AccountVerificationModel
    {
        public string actionJwt { get; set; }
        public string actionToken { get; set; }
        public DateTime validTill { get; set; }
        public DateTime repeatAfter { get; set; }
    }
}
