using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using BackupServiceAPI.Models;
using System.Net;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BackupServiceAPI.Services
{
    public class Mailer
    {        
        SmtpClient smtpClient = new SmtpClient();
        private DbBackupServiceContext _Context { get; set; }
        private IServiceScope _Scope { get; set; }
        public Mailer(/*DbBackupServiceContext context*/IServiceScope scope) {
            
            //_Context = context;
            _Scope = scope;
        }
        public MailMessage WriteMail(string email)
        {
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress(email),
                Subject = "subject why it aint changing",
                Body = "WHY AM I HERE",
                IsBodyHtml = true,
            };
            return mailMessage;
        }
        public void SendMail(MailMessage mail, string originator)
        {
            DbBackupServiceContext dbContext = _Scope.ServiceProvider.GetRequiredService<DbBackupServiceContext>();
            MailMessage toSend = WriteMail(originator);
            List<string> accounts = new List<string>();
            foreach (var Account in dbContext.Accounts.ToList())
            {
                if (Account.Admin)
                    toSend.To.Add(Account.Email);
            }
            smtpClient.Send(toSend);
        }
        public void Setup(string host, int port, string email, string password, bool enableSSL)
        {
            smtpClient.Host = host;
            smtpClient.Port = port;
            smtpClient.Credentials = new NetworkCredential(email, password);
            smtpClient.EnableSsl = enableSSL;
        }
       
    }
    
}