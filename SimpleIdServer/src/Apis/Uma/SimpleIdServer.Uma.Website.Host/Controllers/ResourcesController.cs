using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Website.ResourcesController;
using SimpleIdServer.Uma.Website.Host.Dtos;
using SimpleIdServer.Uma.Website.Host.Extensions;
using SimpleIdServer.Uma.Website.Host.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Website.Host.Controllers
{
    [Authorize("connected")]
    public class ResourcesController : BaseController
    {
        private Dictionary<string, Func<ResourceSet, string>> _mappinNameToCallback = new Dictionary<string, Func<ResourceSet, string>>
        {
            { "picture", (r) => r.IconUri },
            { "name", (r) => r.Name },
            { "type", (r) => r.Type },
            { "uri", (r) => r.Uri }
        };
        private readonly IResourcesActions _resourcesActions;
        private readonly UmaAuthenticationWebsiteOptions _options;

        public ResourcesController(IAuthenticationService  authenticationService, IResourcesActions resourcesActions, UmaAuthenticationWebsiteOptions options) : base(authenticationService)
        {
            _resourcesActions = resourcesActions;
            _options = options;
        }

        #region Actions

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await RefreshViewBag().ConfigureAwait(false);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Shared()
        {
            await RefreshViewBag().ConfigureAwait(false);
            return View();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateResourcePermissionsRequest updateResourcePermissionsRequest)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await _resourcesActions.UpdateResourcePermissions(new UpdateResourcePermissionsParameter
                {
                    ResourceId = updateResourcePermissionsRequest.Id,
                    Subject = subject,
                    Policies = updateResourcePermissionsRequest.Policies == null ? new List<UpdateResourcePermissionParameter>() :
                        updateResourcePermissionsRequest.Policies.Select(p =>
                        new UpdateResourcePermissionParameter
                        {
                            PolicyId = p.PolicyId,
                            RuleIds = p.Rules
                        })
                }).ConfigureAwait(false);
                return new OkResult();
            }
            catch (UmaResourceNotFoundException)
            {
                return new NotFoundResult();
            }
            catch(UmaNotAuthorizedException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (BaseUmaException ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = ex.Code,
                    ErrorDescription = ex.Message
                };
                return new JsonResult(errorResponse)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetMyResources([FromBody] DatatableRequest datatableRequest)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var searchResult = await _resourcesActions.GetCurrentUserResources(new SearchCurrentUserResourceSetParameter
            {
                Count = datatableRequest.Length,
                StartIndex = datatableRequest.Start,
                Owner = subject
            }).ConfigureAwait(false);
            var result = Convert(datatableRequest, _mappinNameToCallback, searchResult);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetSharedResources([FromBody] DatatableRequest datatableRequest)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var searchResult = await _resourcesActions.GetResourcesSharedWith(new SearchSharedResourcesParameter
            {
                Count = datatableRequest.Length,
                StartIndex = datatableRequest.Start,
                Subject = subject
            }).ConfigureAwait(false);
            var result = Convert(datatableRequest, _mappinNameToCallback, searchResult);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSharedLink([FromBody] ShareResourceRequest shareResourceRequest)
        {
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var confirmationCode = await _resourcesActions.ShareResource(new ShareResourceParameter
                {
                    Owner = subject,
                    ResourceId = shareResourceRequest.Id,
                    Scopes = shareResourceRequest.Scopes
                }).ConfigureAwait(false);
                var url = $"{Request.GetAbsoluteUriWithVirtualPath()}{Url.Action("Confirm", "Resources", new { confirmationCode = confirmationCode })}";
                return new OkObjectResult(new ShareResourceResponse
                {
                    Url = url
                });
            }
            catch(UmaResourceNotFoundException)
            {
                return new NotFoundResult();
            }
            catch(UmaNotAuthorizedException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch(BaseUmaException ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = ex.Code,
                    ErrorDescription = ex.Message
                };
                return new JsonResult(errorResponse)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        [HttpGet("/Resources/Confirm/{confirmationCode}")]
        public async Task<IActionResult> Confirm(string confirmationCode)
        {
            await RefreshViewBag().ConfigureAwait(false);
            var subject = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                await _resourcesActions.ConfirmSharedLink(new ConfirmSharedLinkParameter
                {
                    ConfirmationCode = confirmationCode,
                    OpenidProvider = _options.OpenidWellKnownConfigurationUrl,
                    Subject = subject
                }).ConfigureAwait(false);
                return View(new ConfirmViewModel
                {
                    SuccessMessage = "Your are now ready to use the resource"
                });
            }
            catch(Exception e)
            {
                return View(new ConfirmViewModel
                {
                    ErrorMessage = e.Message
                });
            }
        }

        #endregion

        #region Private methods

        private static DatatableResponse Convert(DatatableRequest datatableRequest, Dictionary<string, Func<ResourceSet, string>> mapping, SearchResourceSetResult searchResult)
        {
            var response = new DatatableResponse();
            var data = new List<List<object>>();
            if (searchResult.Content != null)
            {
                foreach(var resource in searchResult.Content)
                {
                    var row = new List<object>();
                    foreach (var column in datatableRequest.Columns.OrderBy(c => c.Data))
                    {
                        if (!mapping.ContainsKey(column.Name))
                        {
                            continue;
                        }

                        row.Add(mapping[column.Name](resource));
                    }

                    row.Add(resource.Scopes);
                    row.Add(resource.Id);
                    var policies = new List<ResourcePolicyResponse>();
                    if (resource.Policies != null)
                    {
                        foreach(var policy in resource.Policies)
                        {
                            policies.Add(new ResourcePolicyResponse
                            {
                                PolicyId = policy.Id,
                                Rules = policy.Rules.Select(r => new ResourcePolicyRuleResponse
                                {
                                    RuleId = r.Id,
                                    Scopes = r.Scopes,
                                    Permissions = r.Claims.Select(c => $"{c.Type} : {c.Value}")
                                })
                            });
                        }
                    }

                    row.Add(policies);
                    data.Add(row);
                }
            }

            return new DatatableResponse
            {
                Draw = datatableRequest.Draw,
                Data = data,
                RecordsFiltered = searchResult.TotalResults,
                RecordsTotal = searchResult.TotalResults
            };
        }

        #endregion
    }
}