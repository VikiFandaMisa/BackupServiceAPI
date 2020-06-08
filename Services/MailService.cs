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

namespace BackupServiceAPI.Services
{
    public class MailService : BackgroundService
    {
        SmtpClient smtpClient = new SmtpClient();
        private DbBackupServiceContext _Context { get; set; }
        private readonly IServiceScopeFactory scopeFactory;

        private Timer _timer;
        public string Email { get; set; }
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
                MailMessage toSend = WriteMail();
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
            Email = email;
            smtpClient.Credentials = new NetworkCredential(email, password);
            smtpClient.EnableSsl = enableSSL;
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