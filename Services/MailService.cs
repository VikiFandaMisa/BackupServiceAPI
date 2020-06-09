using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using BackupServiceAPI.Models;
using System.Net;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BackupServiceAPI.Services
{
    public class MailService : BackgroundService
    {
        private SmtpClient _SmtpClient = new SmtpClient();
        private DbBackupServiceContext _Context { get; set; }
        private readonly IServiceScopeFactory ScopeFactory;
        private readonly IConfiguration _Configuration;
        private Timer _Timer;
        public string Email { get; set; }

        public MailService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            this.ScopeFactory = scopeFactory;
            this._Configuration = configuration;

            _SmtpClient.Host = _Configuration["SMTP:Host"];
            _SmtpClient.Port = Convert.ToInt32(_Configuration["SMTP:Port"]);
            Email = _Configuration["SMTP:Email"];
            _SmtpClient.Credentials = new NetworkCredential(_Configuration["SMTP:Email"], _Configuration["SMTP:Password"]);
            _SmtpClient.EnableSsl = true;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(432000));
        }
        private void DoWork(object state)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                _Context = scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>();
                MailMessage toSend = WriteMail();
                List<string> accounts = new List<string>();
                foreach (var Account in scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>().Accounts.ToList())
                {
                    if (Account.Admin)
                        toSend.To.Add(Account.Email);
                }
                _SmtpClient.Send(toSend);
            }
        }
        public MailMessage WriteMail()
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(Email),
                Subject = "Report for today: " + DateTime.Now,
                Body = GetHtmlBody(),
                IsBodyHtml = true,
            };
            return mailMessage;
        }

         public string GetHtmlBody ()
        {              
            DateTime now = DateTime.Now;  

            string Body ="";
              
            Body += "<h1>Good day sir</h1> <h2  > Report for today " + now + "</h2><br><h3>reports:</h3><br>";  
           
            foreach (LogItem p in GetLogs())
            {
                Body += @"Client: "  + GetHostname(Convert.ToInt32(p.JobID)) + "&#160&#160&#160 " +  "   Template: "+ GetTemplateName(Convert.ToInt32(p.JobID)) +" &#160 &#160 &#160  Log message: " + p.Message + " &#160 &#160 &#160  Date of log: " + p.Date.Date  + "<br><br>";                
            }

            Body +=" <br><h3>DeadClients:</h3><br>";

            foreach (Computer p in GetDeadComputers())
            {
                Body += "Klient " + p.Hostname + "is Dead <br>";
            }

            Body +=" <br><h3>NewClients:</h3><br>";

            foreach(Computer p in GetComputers())            {
                
                Body += "Client: " +  p.Hostname +"<br>";
            }            

            return Body;

            
        }
        private Computer[] GetComputers() {
            return _Context.Computers.ToArray();
        }

        private LogItem[] GetLogs() {
            return _Context.Log.ToArray();
        }

        private Computer[] GetDeadComputers() {
            return _Context.Computers.FromSqlRaw(@"
                SELECT *
                FROM Computers c                 
                where DATEDIFF(NOW(),LastSeen) > 15"
            ).ToArray();
        }







        private string GetTemplateName(int JobID) {            
            
            var a = _Context.Jobs.FromSqlRaw(@"
                SELECT *
                FROM Jobs p
                WHERE ID = " + JobID
            ).ToArray();

            var b =_Context.Templates.FromSqlRaw(@"
            SELECT *
            FROM Templates p
            WHERE ID = " + a[0].TemplateID
            ).ToArray(); 

            return b[0].Name;
        }

        private string GetHostname(int JobID) {            
            var a = _Context.Jobs.FromSqlRaw(@"
                SELECT *
                FROM Jobs p
                WHERE ID = " + JobID
            ).ToArray();

            var b =_Context.Computers.FromSqlRaw(@"
            SELECT *
            FROM Computers p
            WHERE ID = " + a[0].ComputerID
            ).ToArray();  

            return b[0].Hostname; 
        }
    }
}