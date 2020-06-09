using System;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;

using BackupServiceAPI.Models;
using BackupServiceAPI.Services;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class TokenController : ControllerBase {
        private readonly DbBackupServiceContext _Context;
        private readonly ITokenManager _TokenManager;
        private readonly IConfiguration _Configuration;
        private readonly IPasswordHelper _PasswordHelper;

        public TokenController(DbBackupServiceContext context, ITokenManager tokenManager, IConfiguration configuration, IPasswordHelper passwordHelper) {
            _Context = context;
            _TokenManager = tokenManager;
            _Configuration = configuration;
            _PasswordHelper = passwordHelper;
        }

        [HttpDelete]
        public async Task<IActionResult> InvalidateToken() {
            await _TokenManager.InvalidateCurrentToken();

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("user")]
        public IActionResult CreateTokenUser([FromBody] Login login) {
            IActionResult response = Unauthorized();

            var user = AuthenticateAccount(login);

            if (user != null) {
                var tokenString = _BuildToken(GetClaims("user", user.ID));
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("computer")]
        public IActionResult CreateTokenComputer([FromBody] LoginComputer login) {
            IActionResult response = Unauthorized();

            var computer = AuthenticateComputer(login);

            if (computer != null) {
                var tokenString = _BuildToken(GetClaims("computer", computer.ID));
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string _BuildToken(Claim[] claims) {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_Configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _Configuration["Jwt:Issuer"],
                _Configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(
                    Convert.ToInt32(_Configuration["Jwt:DaysValid"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Claim[] GetClaims(string type, int id) {
            return new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub, type + ':' + id.ToString())
            };
        }

        private Account AuthenticateAccount(Login login) {
            var passwordHash = _PasswordHelper.CreatePasswordHash(login.Password);
            return _Context.Accounts.SingleOrDefault(a => a.Email == login.Email && a.Password == passwordHash);
        }

        private Computer AuthenticateComputer(LoginComputer login) {
            return _Context.Computers.SingleOrDefault(a => a.ID == login.ID);
        }
    }
}
