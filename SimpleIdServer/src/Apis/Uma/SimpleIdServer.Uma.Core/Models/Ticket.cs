using System;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Models
{
    public class Ticket
    {
        public string Id { get; set; }
        public IEnumerable<string> Audiences { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
        public int ExpiresIn { get; set; }
        public IEnumerable<TicketLine> Lines { get; set; }
    }
}
