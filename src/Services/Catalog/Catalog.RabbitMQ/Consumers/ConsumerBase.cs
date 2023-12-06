using MassTransit;
using System.Threading.Tasks;

namespace Catalog.RabbitMQ.Consumers
{
    public abstract class ConsumerBase<TMessage> : IConsumer<TMessage>
        where TMessage : class
    {
        /// <summary>
        /// Получить и обработать сообщение
        /// </summary>
        /// <param name="message">Сообщение из очереди</param>
        /// <remarks>Не нужно реализовывать обработку исключений и логирования - это сделано в MassTransit</remarks>
        /// <returns>-</returns>
        public abstract Task ConsumeAsync(MessageContextBase message);

        /// <summary>
        /// Получить и обработать сообщение
        /// </summary>
        /// <param name="context">Сообщение из очереди с контекстом</param>
        /// <returns>-</returns>
        public async Task Consume(ConsumeContext<TMessage> context)
            => await ConsumeAsync(new MessageContextBase(context));

        /// <summary>
        /// Контекст сообщения
        /// </summary>
        public class MessageContextBase
        {
            private readonly ConsumeContext<TMessage> _context;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="context">Контекст из MassTransit</param>
            public MessageContextBase(ConsumeContext<TMessage> context)
                => _context = context;

            /// <summary>
            /// Сообщение
            /// </summary>
            public TMessage Message => _context.Message;
        }
    }
}
