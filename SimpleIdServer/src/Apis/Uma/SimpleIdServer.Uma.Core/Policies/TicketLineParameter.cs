using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core.Policies
{
    public class TicketLineParameter
    {
        public TicketLineParameter(IEnumerable<string> scopes = null)
        {
            Scopes = scopes;
        }

        public IEnumerable<string> Scopes { get; set; }
    }
}
