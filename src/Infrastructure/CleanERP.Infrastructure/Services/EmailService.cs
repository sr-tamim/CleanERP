using Microsoft.Extensions.Logging;
using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = false)
    {
        await SendAsync(new[] { to }, subject, body, isHtml);
    }

    public async Task SendAsync(IEnumerable<string> to, string subject, string body, bool isHtml = false)
    {
        // TODO: Implement actual email sending logic
        // This is a placeholder implementation for demonstration
        
        _logger.LogInformation("Sending email to {Recipients} with subject: {Subject}", 
            string.Join(", ", to), subject);

        // Simulate async operation
        await Task.Delay(100);

        _logger.LogInformation("Email sent successfully");
    }
}
