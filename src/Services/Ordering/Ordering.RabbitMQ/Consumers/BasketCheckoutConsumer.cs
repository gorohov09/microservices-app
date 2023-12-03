using Microsoft.Extensions.Logging;
using Ordering.Domain.Contracts.Messages;
using System.Threading.Tasks;

namespace Ordering.RabbitMQ.Consumers
{
    public class BasketCheckoutConsumer : ConsumerBase<BasketCheckoutIntegrationEvent>
    {
        private readonly ILogger<BasketCheckoutConsumer> _logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger">Логгер</param>
        public BasketCheckoutConsumer(ILogger<BasketCheckoutConsumer> logger)
            => _logger = logger;

        /// <inheritdoc/>
        public override async Task ConsumeAsync(MessageContextBase message)
        {
            await Task.CompletedTask;
            _logger.LogInformation("Сообщение обработано");
            //_logger.LogInformation("Message received {Message}", message.Message.Data);
        }
    }
}
