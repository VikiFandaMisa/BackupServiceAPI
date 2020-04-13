using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

using BackupServiceAPI.Helpers;
using BackupServiceAPI.Models;

namespace BackupServiceAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            AppSettings.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<DbBackupServiceContext>(options =>
                options.UseMySql(
                    string.Format(
                        "Server={0};Database={1};User={2};Password={3};",
                        AppSettings.Configuration["DB:Server"],
                        AppSettings.Configuration["DB:Database"],
                        AppSettings.Configuration["DB:User"],
                        AppSettings.Configuration["DB:Password"]
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
                        ValidIssuer = AppSettings.Configuration["Jwt:Issuer"],
                        ValidAudience = AppSettings.Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(AppSettings.Key)
                    };
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AppSettings.Environment = env;
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("CORSPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
