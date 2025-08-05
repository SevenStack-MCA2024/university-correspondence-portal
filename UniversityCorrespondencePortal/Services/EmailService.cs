using System.Net;
using System.Net.Mail;

namespace UniversityCorrespondencePortal.Services
{
    public class EmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient(); // Loads from Web.config
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpClient.Credentials is NetworkCredential cred ? cred.UserName : "default@domain.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);
                smtpClient.Send(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Log or rethrow - handle failed email sending
                throw new System.Exception("Email sending failed: " + ex.Message);
            }
        }
    }
}
