using Catalog.Core.Abstractions;
using MassTransit;
using System.Threading.Tasks;

namespace Catalog.RabbitMQ.Publishers
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

        public Task PublishWithRoutingKeyAsync<TMessage>(TMessage message, string routingKey) where TMessage : class
        {
            throw new System.NotImplementedException();
        }
    }
}
