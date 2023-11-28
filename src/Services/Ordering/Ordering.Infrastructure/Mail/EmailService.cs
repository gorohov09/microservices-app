using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Models;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        public Task<bool> SendEmail(Email email)
        {
            return Task.FromResult(true); //Заглушка
        }
    }
}
