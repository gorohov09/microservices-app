using MassTransit;
using Ordering.Domain.Abstraction;
using System.Threading.Tasks;

namespace Ordering.RabbitMQ.Publishers
{
    public class MessagePublisherBase : IRabbitMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="publishEndpoint">MassTransit</param>
        public MessagePublisherBase(IPublishEndpoint publishEndpoint)
            => _publishEndpoint = publishEndpoint;

        /// <inheritdoc/>
        public async Task PublishAsync<TMessage>(params TMessage[] messages)
            where TMessage : class
            => await _publishEndpoint.PublishBatch(messages);
    }
}
