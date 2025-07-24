
using Microsoft.Extensions.Configuration;
using OnLineCourse.Core.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OnLineCourse.Service
{

    public interface IEmailNotification
    {
        Task<Response> SendEmailForContactUs(ContactMessage contactMessage);
    }

    public class EmailNotification : IEmailNotification
    {
        private readonly IConfiguration configuration;

        public EmailNotification(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<Response> SendEmailForContactUs(ContactMessage contactMessage)
        {
            var apiKey = configuration["SendGrid:SENDGRID_API_KEY"];
            var from = new EmailAddress(configuration["SendGrid:From"]);
            var to = new EmailAddress(configuration["SendGrid:From"], "Marwan");

            var sendGridMessage = new SendGridMessage()
            {
                From = from,
                ReplyTo = to,
                Subject = "Contact page: Received a request from user"
            };

            sendGridMessage.AddContent(MimeType.Html, GetEmailContent(contactMessage));
            sendGridMessage.AddTo(to);

            Console.WriteLine($"Sending email with payload: \n{sendGridMessage.Serialize()}");

            var response = await new SendGridClient(apiKey).SendEmailAsync(sendGridMessage).ConfigureAwait(false);
            Console.WriteLine($"Response: {response.StatusCode}");
            Console.WriteLine(response.Headers);

            return response;
        }


        private string GetEmailContent(ContactMessage contactMessage)
        {

            return $$"""
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <title>An enquiry received  - {{contactMessage.Subject}}</title>
                    </head>
                    <body>                        
                        <p>Dear LearnSmartCoding</p>
                        <p>You have received an enquiry from a user and the details as follows.</p>
                    
                        <p><strong>Message details</strong></p>
                        <ul>
                            <li>User Name: {{contactMessage.Name}}</li>
                            <li>User Email: {{contactMessage.Email}}</li>
                    <li>Subject: {{contactMessage.Subject}}</li>
                    <li>Message: {{contactMessage.Message}}</li>
                        </ul>
                    
                    
                        <p><strong>Warm regards,</strong></p>
                        <p>LearnSmartCoding [Automated]</p>
                    </body>
                    </html>                    
                    """;
        }


    }
}
