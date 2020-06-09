using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BackupServiceAPI.Services {
    public class TokenManagerMiddleware : IMiddleware {
        private readonly ITokenManager _TokenManager;

        public TokenManagerMiddleware(ITokenManager tokenManager) {
            _TokenManager = tokenManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            if (await _TokenManager.IsCurrentTokenActive()) {
                await next(context);

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
