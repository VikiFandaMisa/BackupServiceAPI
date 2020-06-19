using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

using BackupServiceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BackupServiceAPI.Services {
    public interface ITokenManager {
        Task<bool> IsCurrentTokenActive();
        Task InvalidateCurrentToken();
        Task<bool> IsTokenActive(string token);
        Task InvalidateToken(string token);
        Task<dynamic> GetTokenOwner();
    }

    public class TokenManager : ITokenManager {
        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly DbBackupServiceContext _Context;
        private readonly IConfiguration _Configuration;

        public TokenManager(IHttpContextAccessor httpContextAccessor, DbBackupServiceContext context, IConfiguration configuration) {
            _HttpContextAccessor = httpContextAccessor;
            _Context = context;
            _Configuration = configuration;
        }

        public async Task<bool> IsCurrentTokenActive() {
            return await IsTokenActive(GetCurrentToken());
        }

        public async Task InvalidateCurrentToken() {
            await InvalidateToken(GetCurrentToken());
        }

        public async Task<bool> IsTokenActive(string token) {
            return await _Context.TokenBlacklist.FindAsync(token) == null;
        }

        public async Task InvalidateToken(string token) {
            _Context.TokenBlacklist.Add(new InvalidatedToken() {
                Token = token,
                // Could be less if true expiration would be extracted from token
                Expires = DateTime.Now.AddDays(Convert.ToInt32(_Configuration["JWT:DaysValid"]))
            });
            await _Context.SaveChangesAsync();
        }

        private string GetCurrentToken() {
            var authorizationHeader = _HttpContextAccessor.HttpContext.Request.Headers["authorization"];

            if (authorizationHeader == StringValues.Empty)
                return null;

            return authorizationHeader.Single().Split(' ').Last();
        }

        public async Task<dynamic> GetTokenOwner() {
            var user = _HttpContextAccessor.HttpContext.User;
            if (user.HasClaim(c => c.Type == ClaimTypes.NameIdentifier)) {
                var identifier = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Split(':');
                var type = identifier[0];
                var id = Convert.ToInt32(identifier[1]);

                if (type == "user") {
                    return await _Context.Accounts.AsNoTracking().SingleAsync(item => item.ID == id);
                }

                if (type == "computer") {
                    return await _Context.Computers.AsNoTracking().SingleAsync(item => item.ID == id);
                }
            }
            return null;
        }
    }
}
