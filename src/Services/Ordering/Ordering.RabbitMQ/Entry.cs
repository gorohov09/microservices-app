using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Domain.Contracts.Messages;
using Ordering.RabbitMQ;
using Ordering.RabbitMQ.Consumers;
using RabbitMQ.Client;

namespace Ordering.RabbitMQ
{
    /// <summary>
	/// Класс - входная точка проекта, регистрирующий реализованные зависимости текущим проектом
	/// </summary>
	public static class Entry
    {
        /// <summary>
        /// Добавить службы проекта с очередью
        /// </summary>
        /// <param name="services">Коллекция служб</param>
        /// <param name="connectionString">Строка подключения к RMQ</param>
        /// <returns>Обновленная коллекция служб</returns>
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var hostAddress = configuration["EventBusSettings:HostAddress"];
            var basketCheckoutExchangeName = configuration["EventBusSettings:Consumers:BasketCheckout:ExchangeName"];
            var basketCheckoutQueueName = configuration["EventBusSettings:Consumers:BasketCheckout:QueueName"];
            var exchangeRoutingKey = configuration["EventBusSettings:Consumers:BasketCheckout:ExchangeRoutingKey"];
            var exchangeType = configuration["EventBusSettings:Consumers:BasketCheckout:ExchangeType"];

            services.AddMassTransit(hostAddress, options => options
                .AddConsumer<BasketCheckoutIntegrationEvent, BasketCheckoutConsumer>(
                    queueName: basketCheckoutQueueName, exchangeName: basketCheckoutExchangeName, 
                    exchangeExchangeType: exchangeType, exchangeRoutingKey: exchangeRoutingKey)
                );

            return services;
        }
    }
}
