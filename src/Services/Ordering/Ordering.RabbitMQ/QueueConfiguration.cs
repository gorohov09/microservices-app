using Ordering.RabbitMQ.Consumers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Ordering.RabbitMQ
{
    internal class QueueConfiguration
    {
        private readonly List<Consumer> _consumers;
        private readonly List<Producer> _producers;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connectionString">Строка подключения в формате amqp://user:password@host:5672/vhost</param>
        public QueueConfiguration(string connectionString)
        {
            _consumers = new List<Consumer>();
            _producers = new List<Producer>();
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Сериализатор для сообщений очереди
        /// </summary>
        internal enum Serializer
        {
            /// <summary>
            /// Raw JSON
            /// </summary>
            RawJson = 1,
        }

        /// <summary>
        /// Потребители очереди
        /// </summary>
        public IReadOnlyList<Consumer> Consumers => _consumers;

        /// <summary>
        /// Потребители очереди
        /// </summary>
        public IReadOnlyList<Producer> Producers => _producers;

        /// <summary>
        /// Строка подключения в формате amqp://user:password@host:5672/vhost
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Добавить потребителя очереди
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для потребителя очереди</typeparam>
        /// <typeparam name="THandler">Тип обработчика потребителя очереди</typeparam>
        /// <param name="queueName">Название очереди. С таким же именем будет создан exchange для тестирования очереди отдельно от внешних exchange. По умолчанию {THandler.FullName}Queue.
        /// Для ошибок создастся очередь {THandler.FullName}Queue_error и одноименный exchange.</param>
        /// <param name="queuePrefetchCount">По сколько сообщений загружать за раз. По умолчанию 1</param>
        /// <param name="queueDurable">Сохранять ли в персистентное хранилище данные очереди</param>
        /// <param name="queueAutoDelete">Удалять ли очередь при отключении всех подписчиков</param>
        /// <param name="queueExchangeType">Тип очереди. <see cref="ExchangeType"/></param>
        /// <param name="queueSerializer">Сериализатор сообщений в очереди</param>
        /// <param name="exchangeName">Название exchange. По умолчанию {TMessage.FullName}Exchange</param>
        /// <param name="exchangeDurable">Сохранять ли в персистентное хранилище данные exchange</param>
        /// <param name="exchangeAutoDelete">Удалять ли exchange при отключении всех подписчиков</param>
        /// <param name="exchangeExchangeType">Тип exchange. <see cref="ExchangeType"/></param>
        /// <param name="exchangeRoutingKey">Ключ роутинга для перенаправления сообщений из exchange в очередь</param>
        /// <param name="shouldRetry">Пытаться переотправить сообщение, если упадет</param>
        /// <returns>Конструктор конфигурации</returns>
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
        public QueueConfiguration AddConsumer<TMessage, THandler>(string queueName = null, int queuePrefetchCount = 1,
            bool queueDurable = true, bool queueAutoDelete = false, string queueExchangeType = ExchangeType.Fanout,
            Serializer queueSerializer = Serializer.RawJson, string exchangeName = null,
            bool exchangeDurable = true, bool exchangeAutoDelete = false, string exchangeExchangeType = ExchangeType.Fanout,
            string exchangeRoutingKey = null, bool shouldRetry = false)
            where THandler : ConsumerBase<TMessage>
            where TMessage : class
        {
            queueName ??= $"{typeof(THandler).FullName}Queue";
            exchangeName ??= $"{typeof(TMessage).FullName}Exchange";

            _consumers.Add(new Consumer(
                typeof(THandler), queueName, queuePrefetchCount,
                queueDurable, queueAutoDelete, queueExchangeType,
                queueSerializer, exchangeName,
                exchangeDurable, exchangeAutoDelete, exchangeExchangeType,
                exchangeRoutingKey, shouldRetry));
            return this;
        }
#pragma warning restore SA1117 // Parameters should be on same line or separate lines

        /// <summary>
        /// Добавить производителя очереди
        /// </summary>
        /// <typeparam name="TMessage">Тип сообщения для потребителя очереди</typeparam>
        /// <param name="exchangeName">Название exchange. По умолчанию {TMessage.FullName}Exchange</param>
        /// <param name="exchangeDurable">Сохранять ли в персистентное хранилище данные exchange</param>
        /// <param name="exchangeAutoDelete">Удалять ли exchange при отключении всех подписчиков</param>
        /// <param name="exchangeExchangeType">Тип exchange. <see cref="ExchangeType"/></param>
        /// <returns>Конструктор конфигурации</returns>
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
        public QueueConfiguration AddProducer<TMessage>(
            string exchangeName = null,
            bool exchangeDurable = true, bool exchangeAutoDelete = false, string exchangeExchangeType = ExchangeType.Fanout)
            where TMessage : class
        {
            exchangeName ??= $"{typeof(TMessage).FullName}Exchange";

            _producers.Add(new Producer(
                typeof(TMessage), exchangeName,
                exchangeDurable, exchangeAutoDelete, exchangeExchangeType));
            return this;
        }
#pragma warning restore SA1117 // Parameters should be on same line or separate lines

        /// <summary>
        /// Потребитель очереди
        /// </summary>
        internal class Consumer
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="handlerType">Тип обработчика потребителя очереди</param>
            /// <param name="queueName">Название очереди. С таким же именем будет создан exchange для тестирования очереди отдельно от внешних exchange. По умолчанию {THandler.FullName}Queue.
            /// Для ошибок создастся очередь {THandler.FullName}Queue_error и одноименный exchange.</param>
            /// <param name="queuePrefetchCount">По сколько сообщений загружать за раз. По умолчанию 1</param>
            /// <param name="queueDurable">Сохранять ли в персистентное хранилище данные очереди</param>
            /// <param name="queueAutoDelete">Удалять ли очередь при отключении всех подписчиков</param>
            /// <param name="queueExchangeType">Тип очереди. <see cref="ExchangeType"/></param>
            /// <param name="queueSerializer">Сериализатор сообщений в очереди</param>
            /// <param name="exchangeName">Название exchange. По умолчанию {TMessage.FullName}Exchange</param>
            /// <param name="exchangeDurable">Сохранять ли в персистентное хранилище данные exchange</param>
            /// <param name="exchangeAutoDelete">Удалять ли exchange при отключении всех подписчиков</param>
            /// <param name="exchangeExchangeType">Тип exchange. <see cref="ExchangeType"/></param>
            /// <param name="exchangeRoutingKey">Ключ роутинга для перенаправления сообщений из exchange в очередь</param>
            /// <param name="shouldRetry">Пытаться переотправить сообщение, если упадет</param>
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
            public Consumer(Type handlerType, string queueName = null, int queuePrefetchCount = 1,
                bool queueDurable = true, bool queueAutoDelete = false, string queueExchangeType = ExchangeType.Fanout,
                Serializer queueSerializer = Serializer.RawJson, string exchangeName = null,
                bool exchangeDurable = true, bool exchangeAutoDelete = false, string exchangeExchangeType = ExchangeType.Fanout,
                string exchangeRoutingKey = null, bool shouldRetry = false)
#pragma warning restore SA1117 // Parameters should be on same line or separate lines
            {
                HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
                QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
                ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
                QueuePrefetchCount = queuePrefetchCount;
                QueueDurable = queueDurable;
                QueueAutoDelete = queueAutoDelete;
                QueueExchangeType = queueExchangeType;
                QueueSerializer = queueSerializer;
                ExchangeDurable = exchangeDurable;
                ExchangeAutoDelete = exchangeAutoDelete;
                ExchangeExchangeType = exchangeExchangeType;
                ExchangeRoutingKey = exchangeRoutingKey;
                ShouldRetry = shouldRetry;
            }

            /// <summary>
            /// Тип обработчика потребителя очереди
            /// </summary>
            public Type HandlerType { get; }

            /// <summary>
            /// Пытаться переотправить сообщение, если упадет
            /// </summary>
            public bool ShouldRetry { get; }

            /// <summary>
            /// Название очереди. С таким же именем будет создан exchange для тестирования очереди отдельно от внешних exchange. По умолчанию {THandler.FullName}Queue.
            /// Для ошибок создастся очередь {THandler.FullName}Queue_error и одноименный exchange.
            /// </summary>
            public string QueueName { get; }

            /// <summary>
            /// По сколько сообщений загружать за раз. По умолчанию 1
            /// </summary>
            public int QueuePrefetchCount { get; }

            /// <summary>
            /// Сохранять ли в персистентное хранилище данные очереди
            /// </summary>
            public bool QueueDurable { get; }

            /// <summary>
            /// Удалять ли очередь при отключении всех подписчиков
            /// </summary>
            public bool QueueAutoDelete { get; }

            /// <summary>
            /// Тип очереди. <see cref="ExchangeType"/>
            /// </summary>
            public string QueueExchangeType { get; }

            /// <summary>
            /// Сериализатор сообщений в очереди
            /// </summary>
            public Serializer QueueSerializer { get; }

            /// <summary>
            /// Название exchange. По умолчанию {TMessage.FullName}Exchange
            /// </summary>
            public string ExchangeName { get; }

            /// <summary>
            /// Сохранять ли в персистентное хранилище данные exchange
            /// </summary>
            public bool ExchangeDurable { get; }

            /// <summary>
            /// Удалять ли exchange при отключении всех подписчиков
            /// </summary>
            public bool ExchangeAutoDelete { get; }

            /// <summary>
            /// Тип exchange. <see cref="ExchangeType"/>
            /// </summary>
            public string ExchangeExchangeType { get; }

            /// <summary>
            /// Ключ роутинга для перенаправления сообщений из exchange в очередь
            /// </summary>
            public string ExchangeRoutingKey { get; }
        }

        /// <summary>
        /// Публикатор очереди
        /// </summary>
        internal class Producer
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="messageType">Тип обработчика потребителя очереди</param>
            /// <param name="exchangeName">Название exchange. По умолчанию {TMessage.FullName}Exchange</param>
            /// <param name="exchangeDurable">Сохранять ли в персистентное хранилище данные exchange</param>
            /// <param name="exchangeAutoDelete">Удалять ли exchange при отключении всех подписчиков</param>
            /// <param name="exchangeExchangeType">Тип exchange. <see cref="ExchangeType"/></param>
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
            public Producer(Type messageType, string exchangeName = null,
                bool exchangeDurable = true, bool exchangeAutoDelete = false, string exchangeExchangeType = ExchangeType.Fanout)
#pragma warning restore SA1117 // Parameters should be on same line or separate lines
            {
                MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
                ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
                ExchangeDurable = exchangeDurable;
                ExchangeAutoDelete = exchangeAutoDelete;
                ExchangeExchangeType = exchangeExchangeType;
            }

            /// <summary>
            /// Тип обработчика потребителя очереди
            /// </summary>
            public Type MessageType { get; }

            /// <summary>
            /// Название exchange. По умолчанию {TMessage.FullName}Exchange
            /// </summary>
            public string ExchangeName { get; }

            /// <summary>
            /// Сохранять ли в персистентное хранилище данные exchange
            /// </summary>
            public bool ExchangeDurable { get; }

            /// <summary>
            /// Удалять ли exchange при отключении всех подписчиков
            /// </summary>
            public bool ExchangeAutoDelete { get; }

            /// <summary>
            /// Тип exchange. <see cref="ExchangeType"/>
            /// </summary>
            public string ExchangeExchangeType { get; }
        }
    }
}
