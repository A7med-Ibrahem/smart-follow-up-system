using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SmartFollowUp.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _configuration["Email:FromName"],
                _configuration["Email:Username"]
            ));
            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _configuration["Email:Host"],
                int.Parse(_configuration["Email:Port"]!),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        // Send Activation Email to Patient
        public async Task SendPatientActivationEmailAsync(string toEmail, string patientName, string tempPassword)
        {
            var subject = "Welcome to Smart Follow Up - Activate Your Account";
            var body = $@"
                <h2>Welcome {patientName}!</h2>
                <p>Your doctor has created a follow-up account for you.</p>
                <p><strong>Email:</strong> {toEmail}</p>
                <p><strong>Temporary Password:</strong> {tempPassword}</p>
                <p>Please login and complete your profile.</p>
                <br>
                <p>Smart Follow Up Team</p>
            ";

            await SendEmailAsync(toEmail, patientName, subject, body);
        }

        // Send Password Reset Email
        public async Task SendPasswordResetEmailAsync(string toEmail, string name, string resetToken)
        {
            var subject = "Smart Follow Up - Password Reset";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hi {name},</p>
                <p>Your password reset token is:</p>
                <h3>{resetToken}</h3>
                <p>This token expires in 1 hour.</p>
                <br>
                <p>Smart Follow Up Team</p>
            ";

            await SendEmailAsync(toEmail, name, subject, body);
        }
    }
}