using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackupServiceAPI.Helpers;
using BackupServiceAPI.Models;

namespace BackupServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase {
        private readonly DbBackupServiceContext _context;

        public TokenController(DbBackupServiceContext context) {
            _context = context;
        }
        
        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody]Login login) {
            IActionResult response = Unauthorized();

            Account user = _Authenticate(login);

            if (user != null) {
                string tokenString = _BuildToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string _BuildToken(Account user) {
            Claim[] claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString())
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(AppSettings.Key);
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                AppSettings.Configuration["Jwt:Issuer"],
                AppSettings.Configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(
                    Convert.ToInt32(AppSettings.Configuration["Jwt:DaysValid"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Account _Authenticate(Login login) {
            return _context.Accounts.SingleOrDefault(a => a.Username == login.Username && a.Password == TokenHelper.GetPasswordHash(login.Password));
        }
    }
}
