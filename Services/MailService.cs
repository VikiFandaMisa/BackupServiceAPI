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

namespace BackupServiceAPI.Services {
    public class MailService : BackgroundService {
        private readonly SmtpClient _SmtpClient = new SmtpClient();
        private DbBackupServiceContext _Context;
        private readonly IServiceScopeFactory _ScopeFactory;
        private readonly IConfiguration _Configuration;
        public string Email { get; set; }

        public MailService(IServiceScopeFactory scopeFactory, IConfiguration configuration) {
            _ScopeFactory = scopeFactory;
            _Configuration = configuration;

            _SmtpClient.Host = _Configuration["SMTP:Host"];
            _SmtpClient.Port = Convert.ToInt32(_Configuration["SMTP:Port"]);
            Email = _Configuration["SMTP:Email"];
            _SmtpClient.Credentials = new NetworkCredential(_Configuration["SMTP:Email"], _Configuration["SMTP:Password"]);
            _SmtpClient.EnableSsl = true;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        }
        private void DoWork(object state) {
            using var scope = _ScopeFactory.CreateScope();
            _Context = scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>();
            var toSend = WriteMail();
            var accounts = new List<string>();
            foreach (var account in scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>().Accounts.ToList()) {
                if (account.Admin)
                    toSend.To.Add(account.Email);
            }
            _SmtpClient.Send(toSend);
        }
        public MailMessage WriteMail() {
            var mailMessage = new MailMessage {
                From = new MailAddress(Email),
                Subject = "Report for today: " + DateTime.Now,
                Body = GetHtmlBody(),
                IsBodyHtml = true,
            };
            return mailMessage;
        }

        public string GetHtmlBody() {
            var now = DateTime.Now;

            var body = "";

            body += "<h1>Good day sir</h1> <h2  > Report for today " + now + "</h2><br><h3>reports:</h3><br>";

            foreach (var p in GetLogs()) {
                body += @"Client: " + GetHostname(Convert.ToInt32(p.JobID)) + "&#160&#160&#160 " + "   Template: " + GetTemplateName(Convert.ToInt32(p.JobID)) + " &#160 &#160 &#160  Log message: " + p.Message + " &#160 &#160 &#160  Date of log: " + p.Date.Date + "<br><br>";
            }

            body += " <br><h3>DeadClients:</h3><br>";

            foreach (var c in GetDeadComputers()) {
                body += "Klient " + c.Hostname + "is Dead <br>";
            }

            body += " <br><h3>NewClients:</h3><br>";

            foreach (var c in GetComputers()) {

                body += "Client: " + c.Hostname + "<br>";
            }

            return body;


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







        private string GetTemplateName(int jobID) {

            var a = _Context.Jobs.FromSqlRaw(@"
                SELECT *
                FROM Jobs p
                WHERE ID = " + jobID
            ).ToArray();

            var b = _Context.Templates.FromSqlRaw(@"
            SELECT *
            FROM Templates p
            WHERE ID = " + a[0].TemplateID
            ).ToArray();

            return b[0].Name;
        }

        private string GetHostname(int jobID) {
            var a = _Context.Jobs.FromSqlRaw(@"
                SELECT *
                FROM Jobs p
                WHERE ID = " + jobID
            ).ToArray();

            var b = _Context.Computers.FromSqlRaw(@"
            SELECT *
            FROM Computers p
            WHERE ID = " + a[0].ComputerID
            ).ToArray();

            return b[0].Hostname;
        }
    }
}
