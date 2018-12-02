using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Common.Dtos.Responses;
using SimpleIdServer.Concurrency;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Uma.Common.DTOs;
using SimpleIdServer.Uma.Core.Api.ResourceSetController;
using SimpleIdServer.Uma.Host.Extensions;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.ResourceSet)]
    public class ResourceSetController : Controller
    {
        private readonly IResourceSetActions _resourceSetActions;
        private readonly IRepresentationManager _representationManager;

        public ResourceSetController(
            IResourceSetActions resourceSetActions,
            IRepresentationManager representationManager)
        {
            _resourceSetActions = resourceSetActions;
            _representationManager = representationManager;
        }

        [HttpPost(".search")]
        [Authorize("resources")]
        public async Task<IActionResult> SearchResourceSets([FromBody] SearchResourceSet searchResourceSet)
        {
            if (searchResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = searchResourceSet.ToParameter();
            var result = await _resourceSetActions.Search(parameter);
            return new OkObjectResult(result.ToResponse());
        }

        [HttpGet]
        [Authorize("resources")]
        public async Task<ActionResult> GetResourceSets()
        {
            var resourceSetIds = await _resourceSetActions.GetAllResourceSet().ConfigureAwait(false);
            return new OkObjectResult(resourceSetIds);
        }

        [HttpGet("{id}")]
        [Authorize("resources")]
        public async Task<ActionResult> GetResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, Constants.CachingStoreNames.GetResourceStoreName + id))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = await _resourceSetActions.GetResourceSet(id);
            if (result == null)
            {
                return GetNotFoundResourceSet();
            }

            var content = result.ToResponse();
            await _representationManager.AddOrUpdateRepresentationAsync(this, Constants.CachingStoreNames.GetResourceStoreName + id);
            return new OkObjectResult(content);
        }

        [HttpPost]
        [Authorize("resources")]
        public async Task<ActionResult> AddResourceSet([FromBody] PostResourceSet postResourceSet)
        {
            if (postResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = postResourceSet.ToParameter();
            var result = await _resourceSetActions.AddResourceSet(parameter);
            var response = new AddResourceSetResponse
            {
                Id = result
            };
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPut]
        [Authorize("resources")]
        public async Task<ActionResult> UpdateResourceSet([FromBody] PutResourceSet putResourceSet)
        {
            if (putResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = putResourceSet.ToParameter();
            var resourceSetExists = await _resourceSetActions.UpdateResourceSet(parameter);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            var response = new UpdateResourceSetResponse
            {
                Id = putResourceSet.Id
            };

            await _representationManager.AddOrUpdateRepresentationAsync(this, Constants.CachingStoreNames.GetResourceStoreName + putResourceSet.Id, false);
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        [HttpDelete("{id}")]
        [Authorize("resources")]
        public async Task<ActionResult> DeleteResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            var policyIds = await _resourceSetActions.GetPolicies(id);
            var resourceSetExists = await _resourceSetActions.RemoveResourceSet(id);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            // Update all the representations include the authorization policies
            await _representationManager.AddOrUpdateRepresentationAsync(this, Constants.CachingStoreNames.GetResourceStoreName + id, false);
            foreach (var policyId in policyIds)
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, Constants.CachingStoreNames.GetPolicyStoreName + policyId, false);
            }

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundResourceSet()
        {
            var errorResponse = new ErrorResponse
            {
                Error = "not_found",
                ErrorDescription = "resource cannot be found"
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
