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
        private readonly BackupDBContext _context;

        public TokenController(BackupDBContext context) {
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
            if (AppSettings.Environment.IsDevelopment() &&
                login.Username == "test" &&
                login.Password == "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8") { // "password"
                return new Account() {
                    ID = 99999,
                    Username = "test",
                    Password = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
                    Admin = true,
                    Email = "test@test.test",
                    SendReports = false,
                    ReportPeriod = ""
                };
            }

            return _context.Accounts.SingleOrDefault(a => a.Username == login.Username && a.Password == login.Password);
        }
    }
}
