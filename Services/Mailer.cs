using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using BackupServiceAPI.Models;
using System.Net;

namespace BackupServiceAPI.Services
{
    public interface IMailer {
        void Setup(string host, int port, string email, string password, bool enableSSL);
        void SendMail(MailMessage mail, string originator);
        MailMessage WriteMail(string email);
    }
    public class Mailer : IMailer {        
        SmtpClient smtpClient = new SmtpClient();
        private readonly DbBackupServiceContext _Context;
        public Mailer(DbBackupServiceContext context) {
            _Context = context;
        }
        public void Setup(string host, int port, string email, string password, bool enableSSL) {
            smtpClient.Host = host;
            smtpClient.Port = port;
            smtpClient.Credentials = new NetworkCredential(email, password);
            smtpClient.EnableSsl = enableSSL;
        }
        public void SendMail(MailMessage mail, string originator) {
            MailMessage toSend = WriteMail(originator);
            List<string> accounts = new List<string>();
            foreach (var Account in _Context.Accounts)
            {
                if (Account.Admin)
                    toSend.To.Add(Account.Email);
            }
        }
        public MailMessage WriteMail(string email) {
            var mailMessage = new MailMessage {
                    From = new MailAddress(email),
                    Subject = "subject",
                    Body = "<h1>Hello</h1>",
                    IsBodyHtml = true,
                };
            return mailMessage;
        }        
    }
}