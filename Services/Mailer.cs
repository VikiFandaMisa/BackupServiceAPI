using System.Collections;
using System.Net.Mail;
using System.Net;

namespace BackupServiceAPI.Services
{
    public class Mailer
    {
        SmtpClient smtpClient = new SmtpClient();
        public void Setup(string host, int port, string email, string password, bool enableSSL) {
            smtpClient.Host = host;
            smtpClient.Port = port;
            smtpClient.Credentials = new NetworkCredential(email, password);
            smtpClient.EnableSsl = enableSSL;
        }
        public void SendMail(MailMessage mail, string originator) {
            MailMessage toSend = WriteMail(originator);
            

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