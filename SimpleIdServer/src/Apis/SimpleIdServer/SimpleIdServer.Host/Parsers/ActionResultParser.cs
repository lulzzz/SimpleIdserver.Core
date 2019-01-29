﻿using Microsoft.AspNetCore.Routing;
using SimpleIdServer.Core.Results;

namespace SimpleIdServer.Host.Parsers
{
    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult);
        RouteValueDictionary GetRedirectionParameters(ActionResult actionResult);
    }

    public class ActionResultParser : IActionResultParser
    {
        private readonly IRedirectInstructionParser _redirectInstructionParser;

        public ActionResultParser(IRedirectInstructionParser redirectInstructionParser)
        {
            _redirectInstructionParser = redirectInstructionParser;
        }

        public ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult)
        {
            if (actionResult.Type != TypeActionResult.RedirectToAction 
                || actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetActionInformation(actionResult.RedirectInstruction, actionResult.AmrLst);
        }

        public RouteValueDictionary GetRedirectionParameters(ActionResult actionResult)
        {
            if (actionResult.Type != TypeActionResult.RedirectToAction &&
                actionResult.Type != TypeActionResult.RedirectToCallBackUrl ||
                actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetRouteValueDictionary(actionResult.RedirectInstruction);
        }
    }
}