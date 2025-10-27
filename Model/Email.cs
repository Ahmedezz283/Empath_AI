using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Empath_AI.Model
{
    public class Email
    {
        private readonly IConfiguration _config;

        public Email(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = _config["EmailSettings:Email"];
            var appPassword = _config["EmailSettings:AppPassword"];

            var mail = new MailMessage();
            mail.From = new MailAddress(email);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(email, appPassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}