using System.Threading.Tasks;

namespace Basket.Core.Abstractions
{
    public interface IRabbitMessagePublisher
    {
        /// <summary>
        /// Опубликовать сообщение в очередь
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения</typeparam>
        /// <param name="messages">Сообщения</param>
        /// <returns>-</returns>
        Task PublishAsync<TMessage>(params TMessage[] messages)
            where TMessage : class;

        Task PublishWithRoutingKeyAsync<TMessage>(TMessage message, string routingKey)
            where TMessage : class;
    }
}
