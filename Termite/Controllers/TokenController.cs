using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Microsoft.Extensions.Options;

namespace Termite.Controllers
{
    [Produces("application/json")]
    [Route("api/forge")]
    public class TokenController : Controller
    {
        private readonly Data.Secrets _mySecrets;

        public TokenController(IOptions<Data.Secrets> optionsAccessor)
        {
            _mySecrets = optionsAccessor.Value;
        }

        [HttpGet]
        [Route("api/forge/token")]
        public async Task<string> GetToken()
        {
            // get a read-only token
            // NEVER return write-enabled token to client
            TwoLeggedApi oauthApi = new TwoLeggedApi();
            dynamic bearer = await oauthApi.AuthenticateAsync(
                _mySecrets.FORGE_CLIENT_ID,
                _mySecrets.FORGE_CLIENT_SECRET,
                "client_credentials",
                new Scope[] { Scope.DataRead });

            // return a plain text token (for simplicity)
            //var response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.Content = new StringContent(bearer.access_token, System.Text.Encoding.UTF8, "text/plain");
            //return response;
            return (string)bearer.access_token;
        }


    }
}