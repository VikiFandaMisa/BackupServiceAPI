using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using BackupServiceAPI.Models;
using System.Net;
using System;
using System.Linq;

namespace BackupServiceAPI.Services
{
    public class MailService : BackgroundService
    {
        SmtpClient smtpClient = new SmtpClient();
        private DbBackupServiceContext _Context { get; set; }
        private readonly IServiceScopeFactory scopeFactory;

        private Timer _timer;
        public MailService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            this.Setup("smtp.gmail.com", 587, "mymailservice001@gmail.com", "Aa123456+", true);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(432000));
        }
        private void DoWork(object state)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                _Context = scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>();
                //this.SendMail(this.WriteMail("mymailservice001@gmail.com"), "mymailservice001@gmail.com");
                MailMessage toSend = WriteMail("mymailservice001@gmail.com");
                List<string> accounts = new List<string>();
                foreach (var Account in scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>().Accounts.ToList())
                {
                    if (Account.Admin)
                        toSend.To.Add(Account.Email);
                }
                smtpClient.Send(toSend);
            }
        }
        public void Setup(string host, int port, string email, string password, bool enableSSL)
        {
            smtpClient.Host = host;
            smtpClient.Port = port;
            smtpClient.Credentials = new NetworkCredential(email, password);
            smtpClient.EnableSsl = enableSSL;
        }
        public void SendMail(MailMessage mail, string originator)
        {
            MailMessage toSend = WriteMail(originator);
            List<string> accounts = new List<string>();
            foreach (var Account in _Context.Accounts.ToList())
            {
                if (Account.Admin)
                    toSend.To.Add(Account.Email);
            }
            smtpClient.Send(toSend);
        }
        public MailMessage WriteMail(string email)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = "subject",
                Body = "<h1>Hello</h1>",
                IsBodyHtml = true,
            };
            return mailMessage;
        }
    }
}