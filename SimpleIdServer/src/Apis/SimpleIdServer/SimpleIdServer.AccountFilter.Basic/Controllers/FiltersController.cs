using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.AccountFilter.Basic.Common.Requests;
using SimpleIdServer.AccountFilter.Basic.Common.Responses;
using SimpleIdServer.AccountFilter.Basic.Extensions;
using SimpleIdServer.AccountFilter.Basic.Repositories;
using SimpleIdServer.Common.Dtos.Responses;

namespace SimpleIdServer.AccountFilter.Basic.Controllers
{
    [Route("filters")]
    public class FiltersController : Controller
    {
        private readonly IFilterRepository _filterRepository;

        public FiltersController(IFilterRepository filterRepository)
        {
            _filterRepository = filterRepository;
        }

        #region Public methods

        [HttpGet]
        [Authorize("manage_account_filtering")]
        public async Task<IActionResult> GetFilters()
        {
            var filters = await _filterRepository.GetAll();
            return new OkObjectResult(filters.ToDtos());
        }

        [HttpGet("{id}")]
        [Authorize("manage_account_filtering")]
        public async Task<IActionResult> GetFilter(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildMissingParameter("id");
            }

            var filter = await _filterRepository.Get(id);
            if (filter == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            }

            return new OkObjectResult(filter.ToDto());
        }

        [HttpPost]
        [Authorize("manage_account_filtering")]
        public async Task<IActionResult> AddFilter([FromBody] AddFilterRequest addFilterRequest)
        {
            if (addFilterRequest == null)
            {
                return BuildMissingParameter(nameof(addFilterRequest));
            }

            IActionResult errorResult = null;
            if (!CheckParameter(addFilterRequest, out errorResult))
            {
                return errorResult;
            }
            
            var id = await _filterRepository.Add(addFilterRequest.ToParameter());
            return new OkObjectResult(new AddFilterResponse
            {
                Id = id
            });
        }

        [HttpDelete("{id}")]
        [Authorize("manage_account_filtering")]
        public async Task<IActionResult> DeleteFilter(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildMissingParameter("id");
            }

            var filter = await _filterRepository.Get(id);
            if (filter == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            }

            await _filterRepository.Delete(id);
            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manage_account_filtering")]
        public async Task<IActionResult> UpdateFilter([FromBody] UpdateFilterRequest updateFilterRequest)
        {
            if (updateFilterRequest == null)
            {
                return BuildMissingParameter(nameof(updateFilterRequest));
            }

            IActionResult errorResult = null;
            if (!CheckParameter(updateFilterRequest, out errorResult))
            {
                return errorResult;
            }

            var filter = await _filterRepository.Get(updateFilterRequest.Id);
            if (filter == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            }

            await _filterRepository.Update(updateFilterRequest.ToParameter());
            return new NoContentResult();
        }

        #endregion

        #region Private methods

        private static bool CheckParameter(AddFilterRequest addFilterRequest, out IActionResult actionResult)
        {
            actionResult = null;
            if (string.IsNullOrWhiteSpace(addFilterRequest.Name))
            {
                actionResult = BuildMissingParameter("name");
                return false;
            }

            return true;
        }

        private static bool CheckParameter(UpdateFilterRequest updateFilterRequest, out IActionResult actionResult)
        {
            actionResult = null;
            if (string.IsNullOrWhiteSpace(updateFilterRequest.Id))
            {
                actionResult = BuildMissingParameter("id");
                return false;
            }

            if (string.IsNullOrWhiteSpace(updateFilterRequest.Name))
            {
                actionResult = BuildMissingParameter("name");
                return false;
            }

            return true;
        }

        private static IActionResult BuildMissingParameter(string parameterName)
        {
            var error = new ErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = string.Format("the parameter {0} is missing", parameterName)
            };

            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        #endregion
    }
}
