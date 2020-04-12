using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Environment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace BackupServiceAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public string ApplicationData {get; set;}

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;

            this.ApplicationData = Path.Combine(
                GetFolderPath(SpecialFolder.ApplicationData),
                Path.GetFileNameWithoutExtension(
                    System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                )
            );

            if (!Directory.Exists(this.ApplicationData))
                Directory.CreateDirectory(this.ApplicationData);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Models.BackupDBContext>(opt => opt.UseInMemoryDatabase("BackupDB"));
            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(this.GetKey())
                };
                });

            services.AddMvc();
        }

        private byte[] GetKey() {
            string keyFile = Path.Combine(this.ApplicationData, "key");

            // Check if key exists and matches KeyLenght
            if (File.Exists(keyFile)) {
                byte[] readKey = File.ReadAllBytes(keyFile);
                if (readKey.Length == Convert.ToInt32(Configuration["JWT:KeyLength"]))
                {
                    Console.WriteLine("Using an old {0} bit key", Configuration["JWT:KeyLength"]);
                    return readKey;
                }
            }
            
            // If not create and save a new one
            byte[] key = new byte[Convert.ToInt32(Configuration["JWT:KeyLength"])];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(key);

            File.WriteAllBytes(keyFile, key);

            Console.WriteLine("Created a new {0} bit key to {1}", Configuration["JWT:KeyLength"], keyFile);

            return key;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
