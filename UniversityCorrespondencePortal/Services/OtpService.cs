using System;
using System.Net;
using System.Net.Mail;

namespace UniversityCorrespondencePortal.Services
{
    public class OptService
    {
        public bool SendOtpEmail(string toEmail, string otp)
        {
            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(
                            smtpClient.Credentials is NetworkCredential cred ? cred.UserName : "default@domain.com"
                        ),
                        Subject = "Password Reset OTP",
                        Body = $"<p>Your OTP for password reset is: <strong>{otp}</strong></p>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    smtpClient.Send(mailMessage);
                }
                return true;
            }
            catch (SmtpException ex)
            {
                // Log error (optional)
                throw new Exception("Failed to send OTP email: " + ex.Message);
            }
        }
    }
}



