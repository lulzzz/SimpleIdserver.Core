using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Core.Api.Jwks;
using SimpleIdServer.Dtos.Requests;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Jwks)]
    public class JwksController : Controller
    {
        private readonly IJwksActions _jwksActions;

        public JwksController(IJwksActions jwksActions)
        {
            _jwksActions = jwksActions;
        }

        [HttpGet]
        public async Task<JsonWebKeySet> Get()
        {
            return await _jwksActions.GetJwks();
        }

        [HttpPut]
        public async Task<bool> Put()
        {
            return await _jwksActions.RotateJwks();
        }
    }
}
