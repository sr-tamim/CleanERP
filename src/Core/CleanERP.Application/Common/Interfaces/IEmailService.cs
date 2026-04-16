namespace CleanERP.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = false);
    Task SendAsync(IEnumerable<string> to, string subject, string body, bool isHtml = false);
}
