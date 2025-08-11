using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using EmailService.Models;
using Azure;
using Azure.Storage.Queues.Models;

namespace EmailService.Functions
{
    public class SendWelcomeEmail
    {
        private readonly ILogger<SendWelcomeEmail> _logger;

        public SendWelcomeEmail(ILogger<SendWelcomeEmail> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SendWelcomeEmail))]
        public async Task Run([QueueTrigger("user-registrations", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            _logger.LogInformation("C# Queue trigger function processed: {messageText}", myQueueItem);

            try
            {
                var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(myQueueItem);
                
                if (userRegisteredEvent?.Email == null || userRegisteredEvent?.Username == null)
                {
                    _logger.LogError("Failed to deserialize user registered event or required fields are missing.");
                    return;
                }

                string acsConnectionString = Environment.GetEnvironmentVariable("AcsConnectionString")!;
                string acsSenderEmail = Environment.GetEnvironmentVariable("AcsSenderEmail")!;
                
                if (string.IsNullOrEmpty(acsConnectionString) || string.IsNullOrEmpty(acsSenderEmail))
                {
                    _logger.LogError("AcsConnectionString or AcsSenderEmail is not configured.");
                    return;
                }

                EmailClient emailClient = new EmailClient(acsConnectionString);

                EmailContent emailContent = new EmailContent($"¡Bienvenido a nuestro servicio, {userRegisteredEvent.Username}!");
                emailContent.PlainText = $"Hola {userRegisteredEvent.Username}, te has registrado satisfactoriamente.";
                emailContent.Html = $"<strong>Hola {userRegisteredEvent.Username}, te has registrado satisfactoriamente.</strong>";

                EmailRecipients emailRecipients = new EmailRecipients(new[] { new EmailAddress(userRegisteredEvent.Email) });
                EmailMessage emailMessage = new EmailMessage(acsSenderEmail, emailRecipients, emailContent);
                
                _logger.LogInformation($"Attempting to send email to {userRegisteredEvent.Email}...");
                
                // La operación de envío se obtiene aquí
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);
                
                // El resultado final de la operación se obtiene de emailSendOperation.Value
                EmailSendResult sendResult = emailSendOperation.Value;
                
                _logger.LogInformation($"Email sent successfully. Operation ID: {emailSendOperation.Id}");
                _logger.LogInformation($"Email operation status: {sendResult.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the queue message.");
            }
        }
    }
}