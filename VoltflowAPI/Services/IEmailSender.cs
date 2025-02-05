namespace VoltflowAPI.Services
{
    /// <summary>
    /// Manages sending mails
    /// </summary>
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string body);
    }
}
