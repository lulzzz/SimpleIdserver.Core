using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Website.PendingRequestsController;
using SimpleIdServer.Uma.Website.Host.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Website.Host.Controllers
{
    [Authorize("connected")]
    public class PendingRequestsController : BaseController
    {
        private Dictionary<string, Func<PendingRequest, string>> _mappinNameToCallback = new Dictionary<string, Func<PendingRequest, string>>
        {
            { "picture", (r) => {
                if(r.Resource == null)
                {
                    return null;
                }

                return r.Resource.IconUri;
            } },
            { "name", (r) => {
                if (r.Resource == null)
                {
                    return null;
                }

                return r.Resource.Name;
            } },
            { "scopes", (r) => {
                if (r.Scopes == null)
                {
                    return null;
                }

                return string.Join(",", r.Scopes);
            } },
            { "requester", (r) => r.RequesterSubject },
            { "create_datetime", (r) => r.CreateDateTime.ToString() }
        };

        private readonly IPendingRequestsActions _pendingRequestsActions;

        public PendingRequestsController(IAuthenticationService authenticationService, IPendingRequestsActions pendingRequestsActions) : base(authenticationService)
        {
            _pendingRequestsActions = pendingRequestsActions;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await RefreshViewBag().ConfigureAwait(false);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPendingRequests([FromBody] DatatableRequest datatableRequest)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var searchResult = await _pendingRequestsActions.GetPendingRequests(new GetPendingRequestsParameter
            {
                Count = datatableRequest.Length,
                StartIndex = datatableRequest.Start,
                Subject = subject
            }).ConfigureAwait(false);
            var data = new List<List<object>>();
            if (searchResult.PendingRequests != null)
            {
                foreach(var pendingRequest in searchResult.PendingRequests)
                {
                    var row = new List<object>();
                    foreach (var column in datatableRequest.Columns.OrderBy(c => c.Data))
                    {
                        if (!_mappinNameToCallback.ContainsKey(column.Name))
                        {
                            continue;
                        }

                        row.Add(_mappinNameToCallback[column.Name](pendingRequest));
                    }

                    row.Add(pendingRequest.Id);
                    data.Add(row);
                }
            }

            var response = new DatatableResponse
            {
                Draw = datatableRequest.Draw,
                RecordsFiltered = searchResult.TotalResults,
                RecordsTotal = searchResult.TotalResults,
                Data = data
            };
            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> Accept([FromBody] AcceptPendingRequest request)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await _pendingRequestsActions.Accept(subject, request.PendingRequestId).ConfigureAwait(false);
                return new OkResult();
            }
            catch(UmaPolicyNotFoundException)
            {
                return new NotFoundResult();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] RejectPendingRequest request)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await _pendingRequestsActions.Reject(subject, request.PendingRequestId).ConfigureAwait(false);
                return new OkResult();
            }
            catch (UmaPolicyNotFoundException)
            {
                return new NotFoundResult();
            }
        }
    }
}
