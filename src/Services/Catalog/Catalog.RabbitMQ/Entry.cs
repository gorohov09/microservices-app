using Catalog.Core.Contracts.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Catalog.RabbitMQ
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
            var productPriceUpdatedExchangeName = configuration["EventBusSettings:Producers:PriceUpdated:ExchangeName"];

            services.AddMassTransit(hostAddress, options => options
                .AddProducer<ProductPriceUpdatedIntegrationEvent>(exchangeName: productPriceUpdatedExchangeName));

            return services;
        }
    }
}
