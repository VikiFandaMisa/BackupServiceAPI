using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Text;

using BackupServiceAPI.Models;
using BackupServiceAPI.Services;

namespace BackupServiceAPI {
    public class Startup {
        private readonly IConfiguration _Configuration;
        public Startup(IConfiguration configuration) {
            _Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddTransient<TokenManagerMiddleware>();
            services.AddTransient<ITokenManager, Services.TokenManager>();
            services.AddSingleton<IPasswordHelper, Services.PasswordHelper>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContextPool<DbBackupServiceContext>(options =>
                options.UseMySql(
                    string.Format(
                        "Server={0};Database={1};User={2};Password={3};",
                        _Configuration["DB:Server"],
                        _Configuration["DB:Database"],
                        _Configuration["DB:Username"],
                        _Configuration["DB:Password"]
                    ),
                    mySqlOptions => mySqlOptions.ServerVersion(new System.Version(8, 0, 19), ServerType.MySql)
                )
            );
            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _Configuration["JWT:Issuer"],
                        ValidAudience = _Configuration["JWT:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_Configuration["JWT:Key"]))
                    };
                });
            
            services.AddAuthorization(options => {
                options.AddPolicy("UsersOnly", policy =>
                    policy.RequireAssertion(context => 
                        context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier) &&
                        context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Split(':')[0] == "user"
                    )
                );
                options.AddPolicy("ComputersOnly", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier) &&
                        context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Split(':')[0] == "computer"
                    )
                );
            });
            
            services.AddCors(options => {
                options.AddPolicy("CORSPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .Build()
                );
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {   
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CORSPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseMiddleware<TokenManagerMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
