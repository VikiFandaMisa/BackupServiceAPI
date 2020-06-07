using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using BackupServiceAPI.Models;
using System.Net;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BackupServiceAPI.Services
{
    public class Mailer
    {
        private string B = "";
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
                Subject = "subject",
                Body = B,
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
        public void HtmlBody ()
        {              
            DateTime now = DateTime.Now;  

            string Body ="";
              
            Body += "<h1>Good day sir</h1> <h2> Report for today " + now + "</h2><br><h3>reports:</h3><br>";  
           
            foreach(Computer p in GetComputers())            {
                
                Body += p.Hostname +"<br>";
            }

            Body +=" <br><h3><Dead_Clients:</h3><br>";

            foreach (LogItem p in GetLogs())
            {
                Body += "Job " + p.JobID + " message: " + p.Message + "<br>";
            }

            Body +=" <br><h3>NewClients:</h3><br>";

            foreach (Computer p in GetDeadComputers())
            {
                Body += "Klient " + p.Hostname + "is Dead <br>";
            }

            B = Body; 

            
        }
        private Computer[] GetComputers() {
            return _Context.Computers.FromSqlRaw(@"
                SELECT *
                FROM Computers "
            ).ToArray();
        }

        private LogItem[] GetLogs() {
            return _Context.Log.FromSqlRaw(@"
                SELECT *
                FROM Log "
            ).ToArray();
        }

        private Computer[] GetDeadComputers() {
            return _Context.Computers.FromSqlRaw(@"
                SELECT *
                FROM Computers c                 
                where DATEDIFF(NOW(),LastSeen) > 15"
            ).ToArray();
        }
    }
    
}