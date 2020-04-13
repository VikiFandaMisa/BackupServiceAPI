using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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

            var user = _Authenticate(login);

            if (user != null) {
                var tokenString = _BuildToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string _BuildToken(Account user) {
            var key = new SymmetricSecurityKey(AppSettings.Key);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                AppSettings.Configuration["Jwt:Issuer"],
                AppSettings.Configuration["Jwt:Issuer"],
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Account _Authenticate(Login login) {
            return _context.Accounts.SingleOrDefault(a => a.Username == login.Username && a.Password == login.Password);
        }
    }
}
