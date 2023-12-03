using MassTransit.RabbitMqTransport.Topology;
using MassTransit.RabbitMqTransport;
using MassTransit.Topology;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using RabbitMQ.Client;
using GreenPipes;
using Ordering.Domain.Abstraction;
using Ordering.RabbitMQ;
using Ordering.RabbitMQ.Publishers;
using static Ordering.RabbitMQ.QueueConfiguration;

namespace Ordering.RabbitMQ
{
    /// <summary>
	/// Регистратор MassTransit.
	/// see https://masstransit-project.com/
	/// </summary>
	internal static class MassTransitRegistrar
    {
        /// <summary>
        /// Вызвать config.Message{messageType}(x => x.SetEntityName(exchangeName));
        /// </summary>
        /// <param name="config">Конфигурация публикатора</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="exchangeName">Название exchange</param>
        internal static void SetExchangeName(this IRabbitMqBusFactoryConfigurator config, Type messageType, string exchangeName)
        {
            var actionParameterType = typeof(IMessageTopologyConfigurator<>).MakeGenericType(messageType);
            var setEntityNameMethod = actionParameterType!.GetMethod(nameof(IMessageTopologyConfigurator<string>.SetEntityName));

            var p = Expression.Parameter(actionParameterType, "x");
            var expression = Expression.Lambda(
                                Expression.GetActionType(actionParameterType),
                                Expression.Call(p, setEntityNameMethod!, Expression.Constant(exchangeName)),
                                p);

            typeof(IBusFactoryConfigurator).GetMethod(nameof(IBusFactoryConfigurator.Message))
                !.MakeGenericMethod(messageType)
                .Invoke(config, new object[] { expression.Compile() });
        }

        /// <summary>
        /// Вызвать
        /// config.Publish{messageType}(x =>
        /// {
        ///     x.Durable = durable;
        ///     x.AutoDelete = autoDelete;
        ///     x.ExchangeType = exchangeType;
        /// });
        /// </summary>
        /// <param name="config">Конфигурация публикатора</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="durable">Сохранять ли exchange в персистентное хранилище</param>
        /// <param name="autoDelete">Удалять ли при отключении всех каналов</param>
        /// <param name="exchangeType">Тип exchange</param>
        internal static void ConfigureExchange(this IRabbitMqBusFactoryConfigurator config, Type messageType, bool durable, bool autoDelete, string exchangeType)
        {
            var actionParameterType = typeof(IRabbitMqMessagePublishTopologyConfigurator<>).MakeGenericType(messageType);
            var propertyDeclaredInterface = typeof(IExchangeConfigurator);
            var autoDeleteProperty = propertyDeclaredInterface!.GetProperty(nameof(IExchangeConfigurator.AutoDelete))!.SetMethod!;
            var durableProperty = propertyDeclaredInterface!.GetProperty(nameof(IExchangeConfigurator.Durable))!.SetMethod!;
            var exchangeTypeProperty = propertyDeclaredInterface!.GetProperty(nameof(IExchangeConfigurator.ExchangeType))!.SetMethod!;

            var p = Expression.Parameter(actionParameterType, "x");
            var autoDeleteCall = Expression.Call(p, autoDeleteProperty, Expression.Constant(autoDelete));
            var durableCall = Expression.Call(p, durableProperty, Expression.Constant(durable));
            var exchangeTypeCall = Expression.Call(p, exchangeTypeProperty, Expression.Constant(exchangeType));

            var block = Expression.Block(new Expression[] { autoDeleteCall, durableCall, exchangeTypeCall });

            var expression = Expression.Lambda(
                                Expression.GetActionType(actionParameterType),
                                block,
                                p);

            typeof(IRabbitMqBusFactoryConfigurator).GetMethod(nameof(IRabbitMqBusFactoryConfigurator.Publish))
                !.MakeGenericMethod(messageType)
                .Invoke(config, new object[] { expression.Compile() });
        }

        /// <summary>
        /// Зарегать подключение к RMQ через MassTransit.
        /// see https://masstransit-project.com/
        /// </summary>
        /// <param name="services">Коллекция служб приложения</param>
        /// <param name="connectionString">Строка подключения к RMQ</param>
        /// <param name="options">Параметры подписок и публикации сообщений</param>
        /// <returns>Коллекция служб приложения с MassTransit</returns>
        internal static IServiceCollection AddMassTransit(
            this IServiceCollection services,
            string connectionString,
            Action<QueueConfiguration> options = null)
        {
            var configuration = new QueueConfiguration(connectionString);
            options?.Invoke(configuration);
            return Register(services, configuration)
                .AddTransient<IRabbitMessagePublisher, MessagePublisherBase>();
        }

        private static void ApplySerializer(IReceiveEndpointConfigurator configurator, Serializer serializer)
        {
            if (configurator is null)
                throw new ArgumentNullException(nameof(configurator));

            switch (serializer)
            {
                case Serializer.RawJson:
                    configurator.ClearMessageDeserializers();
                    configurator.UseRawJsonSerializer();
                    break;
                default:
                    throw new NotImplementedException($"{serializer} not implemented yet");
            }
        }

        private static IServiceCollection Register(IServiceCollection services, QueueConfiguration configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            // IConnection для healthcheck'ов
            services.AddTransient(sp => new ConnectionFactory() { Uri = new Uri(configuration.ConnectionString) }.CreateConnection());

            services.AddMassTransit(massTransit =>
            {
                foreach (var consumer in configuration.Consumers)
                    massTransit.AddConsumer(consumer.HandlerType);

                massTransit.UsingRabbitMq((context, config) =>
                {
                    config.Host(configuration.ConnectionString);

                    // пока конвенциональные подписки не используем, страхово
                    // TODO: подумать в эту сторону
                    // config.ConfigureEndpoints(context);
                    config.ClearMessageDeserializers();
                    config.UseRawJsonSerializer();

                    foreach (var producer in configuration.Producers)
                    {
                        config.SetExchangeName(producer.MessageType, producer.ExchangeName);
                        config.ConfigureExchange(producer.MessageType, producer.ExchangeDurable, producer.ExchangeAutoDelete, producer.ExchangeExchangeType);
                    }

                    // регистрация вручную, чтоб можно было подписываться на существующие уже очереди
                    foreach (var consumer in configuration.Consumers)
                        config.ReceiveEndpoint(consumer.QueueName, queue =>
                        {
                            ApplySerializer(queue, consumer.QueueSerializer);

                            queue.PrefetchCount = consumer.QueuePrefetchCount;
                            queue.Durable = consumer.QueueDurable;
                            queue.AutoDelete = consumer.QueueAutoDelete;
                            queue.ExchangeType = consumer.QueueExchangeType;
                            queue.ConfigureConsumer(context, consumer.HandlerType);

                            if (consumer.ShouldRetry || true)
                                queue.UseRetry(x => x.Intervals(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10)));

                            // нам не нужен exchange для consumer.HandlerType, задаем его вручную
                            queue.ConfigureConsumeTopology = false;
                            queue.Bind(consumer.ExchangeName, exchange =>
                            {
                                exchange.Durable = consumer.ExchangeDurable;
                                exchange.AutoDelete = consumer.ExchangeAutoDelete;
                                exchange.ExchangeType = consumer.ExchangeExchangeType;

                                if (!string.IsNullOrWhiteSpace(consumer.ExchangeRoutingKey))
                                    exchange.RoutingKey = consumer.ExchangeRoutingKey;
                            });
                        });
                });
            });
            services.AddMassTransitHostedService();

            return services;
        }
    }
}
