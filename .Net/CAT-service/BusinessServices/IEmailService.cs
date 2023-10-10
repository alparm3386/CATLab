namespace CAT.Services.Common
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}