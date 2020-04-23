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
        [HttpPost("user")]
        public IActionResult CreateTokenUser([FromBody]Login login) {
            IActionResult response = Unauthorized();

            Account user = _AuthenticateAccount(login);

            if (user != null) {
                string tokenString = _BuildToken(GetClaimsAccount(user));
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("computer")]
        public IActionResult CreateTokenComputer([FromBody]LoginComputer login) {
            IActionResult response = Unauthorized();

            Computer computer = _AuthenticateComputer(login);

            if (computer != null) {
                string tokenString = _BuildToken(GetClaimsComputer(computer));
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string _BuildToken(Claim[] claims) {
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
        
        private Claim[] GetClaimsAccount(Account user) {
            return new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub, string.Format("user:{0}", user.ID.ToString()))
            };
        }

        private Claim[] GetClaimsComputer(Computer computer) {
            return new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub, string.Format("computer:{0}", computer.ID.ToString()))
            };
        }

        private Account _AuthenticateAccount(Login login) {
            string passwordHash = TokenHelper.GetPasswordHash(login.Password);
            return _context.Accounts.SingleOrDefault(a => a.Username == login.Username && a.Password == passwordHash);
        }

        private Computer _AuthenticateComputer(LoginComputer login) {
            return _context.Computers.SingleOrDefault(a => a.ID == login.ID);
        }
    }
}
