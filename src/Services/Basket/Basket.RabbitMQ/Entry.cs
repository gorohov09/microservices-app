﻿using Basket.Core.Contracts.Messages;
using Basket.RabbitMQ.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Basket.RabbitMQ
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
            var basketCheckoutExchangeName = configuration["EventBusSettings:Producers:BasketCheckout:ExchangeName"];

            var priceUpdatedExchangeName = configuration["EventBusSettings:Consumers:PriceUpdated:ExchangeName"];
            var priceUpdatedExchangeType = configuration["EventBusSettings:Consumers:PriceUpdated:ExchangeType"];
            var priceUpdatedQueueName = configuration["EventBusSettings:Consumers:PriceUpdated:QueueName"];

            services.AddMassTransit(hostAddress, options => options
                .AddProducer<BasketCheckoutIntegrationEvent>(exchangeName: basketCheckoutExchangeName, exchangeExchangeType: ExchangeType.Direct)
                .AddConsumer<ProductPriceUpdatedIntegrationEvent, ProductPriceUpdateConsumer>(
                    queueName: priceUpdatedQueueName, 
                    exchangeName: priceUpdatedExchangeName, exchangeExchangeType: priceUpdatedExchangeType));

            return services;
        }
    }
}
