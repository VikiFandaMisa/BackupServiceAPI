using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using BackupServiceAPI.Helpers;
using BackupServiceAPI.Models;

namespace BackupServiceAPI.Services
{
    public class TokenManager : ITokenManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DbBackupServiceContext _dbContext;

        public TokenManager(IHttpContextAccessor httpContextAccessor, DbBackupServiceContext dbContext) {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<bool> IsCurrentTokenActive() {
            return await IsTokenActive(GetCurrentToken());
        }

        public async Task InvalidateCurrentToken() {
            await InvalidateToken(GetCurrentToken());
        }

        public async Task<bool> IsTokenActive(string token) {
            return await _dbContext.TokenBlacklist.FindAsync(token) == null;
        }

        public async Task InvalidateToken(string token) {
            _dbContext.TokenBlacklist.Add(new InvalidatedToken() {
                Token = token,
                // Could be less if true expiration would be extracted from token
                Expires = DateTime.Now.AddDays(Convert.ToInt32(AppSettings.Configuration["JWT:DaysValid"]))
            });
            await _dbContext.SaveChangesAsync();
        }
            
        private string GetCurrentToken()
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["authorization"];

            if (authorizationHeader == StringValues.Empty)
                return null;

            return authorizationHeader.Single().Split(' ').Last();
        }
    }
}