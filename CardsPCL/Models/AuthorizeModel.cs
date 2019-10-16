using System;
using System.Collections.Generic;

namespace CardsPCL.Models
{
    public class AuthorizeModel
    {
        public class Limitations
        {
            public int? invitationsRemaining { get; set; }
            public int? cardsRemaining { get; set; }
            public bool allowMultiClients {get;set;}
        }
    }
}
