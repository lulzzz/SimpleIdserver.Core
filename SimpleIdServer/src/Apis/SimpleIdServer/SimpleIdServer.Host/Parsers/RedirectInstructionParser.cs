using Microsoft.AspNetCore.Routing;
using SimpleIdServer.Core.Results;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdServer.Host.Parsers
{
    public interface IRedirectInstructionParser
    {
        ActionInformation GetActionInformation(RedirectInstruction action, ICollection<string> amrLst);
        RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction);
    }

    public class RedirectInstructionParser : IRedirectInstructionParser
    {
        private readonly Dictionary<IdentityServerEndPoints, ActionInformation> _mappingEnumToActionInformations = new Dictionary<IdentityServerEndPoints, ActionInformation>
        {
            {
                IdentityServerEndPoints.ConsentIndex,
                new ActionInformation("Consent", "Index", "Shell")
            }, 
            {
                IdentityServerEndPoints.FormIndex,
                new ActionInformation("Form", "Index", "Shell")
            }
        };

        public ActionInformation GetActionInformation(RedirectInstruction instruction, ICollection<string> amrLst = null)
        {
            ActionInformation actionInformation = null;
            if (instruction.Action == IdentityServerEndPoints.AuthenticateIndex)
            {
                actionInformation = new ActionInformation("Authenticate", "OpenId", amrLst.Last());
            }
            else
            {
                actionInformation = _mappingEnumToActionInformations[instruction.Action];
            }

            var dic = GetRouteValueDictionary(instruction);
            actionInformation.RouteValueDictionary = dic;
            return actionInformation;
        }

        public RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction)
        {
            var result = new RouteValueDictionary();
            if (instruction.Parameters != null && instruction.Parameters.Any())
            {
                foreach (var parameter in instruction.Parameters)
                {
                    result.Add(parameter.Name, parameter.Value);
                }
            }

            return result;
        }
    }
}