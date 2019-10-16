using System;
namespace CardsPCL.Models
{
    public class AccountActionsGetModel
    {
        public string accountClientJwt { get; set; }
        public DateTime? acknowledged { get; set; }
        public DateTime? processed { get; set; }
        public string accountClientToken { get; set; }
    }
}
